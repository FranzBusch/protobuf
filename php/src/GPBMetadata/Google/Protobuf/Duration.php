<?php
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# NO CHECKED-IN PROTOBUF GENCODE
# source: google/protobuf/duration.proto

namespace GPBMetadata\Google\Protobuf;

class Duration
{
    public static $is_initialized = false;

    public static function initOnce() {
        $pool = \Google\Protobuf\Internal\DescriptorPool::getGeneratedPool();

        if (static::$is_initialized == true) {
          return;
        }
        $pool->internalAddGeneratedFile(
            "\x0A\xEB\x01\x0A\x1Egoogle/protobuf/duration.proto\x12\x0Fgoogle.protobuf\"*\x0A\x08Duration\x12\x0F\x0A\x07seconds\x18\x01 \x01(\x03\x12\x0D\x0A\x05nanos\x18\x02 \x01(\x05B\x83\x01\x0A\x13com.google.protobufB\x0DDurationProtoP\x01Z1google.golang.org/protobuf/types/known/durationpb\xF8\x01\x01\xA2\x02\x03GPB\xAA\x02\x1EGoogle.Protobuf.WellKnownTypesb\x06proto3"
        , true);

        static::$is_initialized = true;
    }
}

