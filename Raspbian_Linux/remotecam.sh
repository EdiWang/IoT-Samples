#!/usr/bin/env bash

raspivid -o - -t 0 -hf -b 1000000 -w 1366 -h 768 -fps 24 |cvlc -vvv stream:///dev/stdin --sout '#standard{access=http,mux=ts,dst=:8160}' :demux=h264

