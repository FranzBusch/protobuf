/*
 * Copyright (c) 2009-2021, Google LLC
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Google LLC nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL Google LLC BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#ifndef UPB_MEM_ARENA_INTERNAL_H_
#define UPB_MEM_ARENA_INTERNAL_H_

#include "upb/mem/arena.h"

// Must be last.
#include "upb/port/def.inc"

typedef struct _upb_MemBlock _upb_MemBlock;

struct upb_Arena {
  _upb_ArenaHead head;
  /* Stores cleanup metadata for this arena.
   * - a pointer to the current cleanup counter.
   * - a boolean indicating if there is an unowned initial block.  */
  uintptr_t cleanup_metadata;

  /* Allocator to allocate arena blocks.  We are responsible for freeing these
   * when we are destroyed. */
  upb_alloc* block_alloc;
  uint32_t last_size;

  /* When multiple arenas are fused together, each arena points to a parent
   * arena (root points to itself). The root tracks how many live arenas
   * reference it.
   *
   * The low bit is tagged:
   *   0: pointer to parent
   *   1: count, left shifted by one
   */
  UPB_ATOMIC uintptr_t parent_or_count;

  /* Linked list of blocks to free/cleanup. */
  _upb_MemBlock *freelist, *freelist_tail;
};

UPB_INLINE bool _upb_Arena_IsTaggedRefcount(uintptr_t parent_or_count) {
  return (parent_or_count & 1) == 1;
}

UPB_INLINE bool _upb_Arena_IsTaggedPointer(uintptr_t parent_or_count) {
  return (parent_or_count & 1) == 0;
}

UPB_INLINE uint32_t _upb_Arena_RefCountFromTagged(uintptr_t parent_or_count) {
  UPB_ASSERT(_upb_Arena_IsTaggedRefcount(parent_or_count));
  return parent_or_count >> 1;
}

UPB_INLINE uintptr_t _upb_Arena_TaggedFromRefcount(uint32_t refcount) {
  uintptr_t parent_or_count = (((uintptr_t)refcount) << 1) | 1;
  UPB_ASSERT(_upb_Arena_IsTaggedRefcount(parent_or_count));
  return parent_or_count;
}

UPB_INLINE upb_Arena* _upb_Arena_PointerFromTagged(uintptr_t parent_or_count) {
  UPB_ASSERT(_upb_Arena_IsTaggedPointer(parent_or_count));
  return (upb_Arena*)parent_or_count;
}

UPB_INLINE uintptr_t _upb_Arena_TaggedFromPointer(upb_Arena* a) {
  uintptr_t parent_or_count = (uintptr_t)a;
  UPB_ASSERT(_upb_Arena_IsTaggedPointer(parent_or_count));
  return parent_or_count;
}

#include "upb/port/undef.inc"

#endif /* UPB_MEM_ARENA_INTERNAL_H_ */
