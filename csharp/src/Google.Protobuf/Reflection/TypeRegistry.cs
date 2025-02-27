﻿#region Copyright notice and license
// Protocol Buffers - Google's data interchange format
// Copyright 2015 Google Inc.  All rights reserved.
//
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file or at
// https://developers.google.com/open-source/licenses/bsd
#endregion

using System.Collections.Generic;
using System.Linq;

namespace Google.Protobuf.Reflection
{
    /// <summary>
    /// An immutable registry of types which can be looked up by their full name.
    /// </summary>
    public sealed class TypeRegistry
    {
        /// <summary>
        /// An empty type registry, containing no types.
        /// </summary>
        public static TypeRegistry Empty { get; } = new TypeRegistry(new Dictionary<string, MessageDescriptor>());

        private readonly Dictionary<string, MessageDescriptor> fullNameToMessageMap;

        private TypeRegistry(Dictionary<string, MessageDescriptor> fullNameToMessageMap)
        {
            this.fullNameToMessageMap = fullNameToMessageMap;
        }

        /// <summary>
        /// Attempts to find a message descriptor by its full name.
        /// </summary>
        /// <param name="fullName">The full name of the message, which is the dot-separated
        /// combination of package, containing messages and message name</param>
        /// <returns>The message descriptor corresponding to <paramref name="fullName"/> or null
        /// if there is no such message descriptor.</returns>
        public MessageDescriptor Find(string fullName)
        {
            // Ignore the return value as ret will end up with the right value either way.
            fullNameToMessageMap.TryGetValue(fullName, out MessageDescriptor ret);
            return ret;
        }

        /// <summary>
        /// Creates a type registry from the specified set of file descriptors.
        /// </summary>
        /// <remarks>
        /// This is a convenience overload for <see cref="FromFiles(IEnumerable{FileDescriptor})"/>
        /// to allow calls such as <c>TypeRegistry.FromFiles(descriptor1, descriptor2)</c>.
        /// </remarks>
        /// <param name="fileDescriptors">The set of files to include in the registry. Must not contain null values.</param>
        /// <returns>A type registry for the given files.</returns>
        public static TypeRegistry FromFiles(params FileDescriptor[] fileDescriptors)
        {
            return FromFiles((IEnumerable<FileDescriptor>) fileDescriptors);
        }

        /// <summary>
        /// Creates a type registry from the specified set of file descriptors.
        /// </summary>
        /// <remarks>
        /// All message types within all the specified files are added to the registry, and
        /// the dependencies of the specified files are also added, recursively.
        /// </remarks>
        /// <param name="fileDescriptors">The set of files to include in the registry. Must not contain null values.</param>
        /// <returns>A type registry for the given files.</returns>
        public static TypeRegistry FromFiles(IEnumerable<FileDescriptor> fileDescriptors)
        {
            ProtoPreconditions.CheckNotNull(fileDescriptors, nameof(fileDescriptors));
            var builder = new Builder();
            foreach (var file in fileDescriptors)
            {
                builder.AddFile(file);
            }
            return builder.Build();
        }

        /// <summary>
        /// Creates a type registry from the file descriptor parents of the specified set of message descriptors.
        /// </summary>
        /// <remarks>
        /// This is a convenience overload for <see cref="FromMessages(IEnumerable{MessageDescriptor})"/>
        /// to allow calls such as <c>TypeRegistry.FromFiles(descriptor1, descriptor2)</c>.
        /// </remarks>
        /// <param name="messageDescriptors">The set of message descriptors to use to identify file descriptors to include in the registry.
        /// Must not contain null values.</param>
        /// <returns>A type registry for the given files.</returns>
        public static TypeRegistry FromMessages(params MessageDescriptor[] messageDescriptors)
        {
            return FromMessages((IEnumerable<MessageDescriptor>) messageDescriptors);
        }

        /// <summary>
        /// Creates a type registry from the file descriptor parents of the specified set of message descriptors.
        /// </summary>
        /// <remarks>
        /// The specified message descriptors are only used to identify their file descriptors; the returned registry
        /// contains all the types within the file descriptors which contain the specified message descriptors (and
        /// the dependencies of those files), not just the specified messages.
        /// </remarks>
        /// <param name="messageDescriptors">The set of message descriptors to use to identify file descriptors to include in the registry.
        /// Must not contain null values.</param>
        /// <returns>A type registry for the given files.</returns>
        public static TypeRegistry FromMessages(IEnumerable<MessageDescriptor> messageDescriptors)
        {
            ProtoPreconditions.CheckNotNull(messageDescriptors, nameof(messageDescriptors));
            return FromFiles(messageDescriptors.Select(md => md.File));
        }

        /// <summary>
        /// Builder class which isn't exposed, but acts as a convenient alternative to passing round two dictionaries in recursive calls.
        /// </summary>
        private class Builder
        {
            private readonly Dictionary<string, MessageDescriptor> types;
            private readonly HashSet<string> fileDescriptorNames;

            internal Builder()
            {
                types = new Dictionary<string, MessageDescriptor>();
                fileDescriptorNames = new HashSet<string>();
            }

            internal void AddFile(FileDescriptor fileDescriptor)
            {
                if (!fileDescriptorNames.Add(fileDescriptor.Name))
                {
                    return;
                }
                foreach (var dependency in fileDescriptor.Dependencies)
                {
                    AddFile(dependency);
                }
                foreach (var message in fileDescriptor.MessageTypes)
                {
                    AddMessage(message);
                }
            }

            private void AddMessage(MessageDescriptor messageDescriptor)
            {
                foreach (var nestedType in messageDescriptor.NestedTypes)
                {
                    AddMessage(nestedType);
                }
                // This will overwrite any previous entry. Given that each file should
                // only be added once, this could be a problem such as package A.B with type C,
                // and package A with type B.C... it's unclear what we should do in that case.
                types[messageDescriptor.FullName] = messageDescriptor;
            }

            internal TypeRegistry Build()
            {
                return new TypeRegistry(types);
            }
        }
    }
}
