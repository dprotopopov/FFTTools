import math
import numpy as np
from enum import Enum


class BuilderBase:

    def __init__(self):
        pass

    def Forward(self, data: np.array):
        return np.fft.fftn(data.astype(complex))

    def Backward(self, data: np.array):
        return np.absolute(np.fft.ifftn(data))

    def Split(self, data: np.array, outer: np.array, middle: np.array, inner: np.array, height: int, width: int):
        s0 = max(0, math.floor((data.shape[0]-height)/2))
        s1 = max(0, math.floor((data.shape[1]-width)/2))
        e0 = min(data.shape[0], math.ceil((data.shape[0]+height)/2))
        e1 = min(data.shape[1], math.ceil((data.shape[1]+width)/2))

        np.resize(outer, data.shape)
        np.resize(middle, data.shape)
        np.resize(inner, data.shape)

        outer.fill(0)
        middle.fill(0)
        inner.fill(0)

        inner[s0:e0, s1:e1] = data[s0:e0, s1:e1]

        outer[:s0, :s1] = data[:s0, :s1]
        outer[:s0, e1:] = data[:s0, e1:]
        outer[e0:, :s1] = data[e0:, :s1]
        outer[e0:, e1:] = data[e0:, e1:]

        middle[:s0, s1:e1] = data[:s0, s1:e1]
        middle[e0:, s1:e1] = data[e0:, s1:e1]
        middle[s0:e0, :s1] = data[s0:e0, :s1]
        middle[s0:e0, e1:] = data[s0:e0, e1:]

    def BlindInner(self, data: np.array, height: int, width: int):
        s0 = max(0, math.floor((data.shape[0]-height)/2))
        s1 = max(0, math.floor((data.shape[1]-width)/2))
        e0 = min(data.shape[0], math.ceil((data.shape[0]+height)/2))
        e1 = min(data.shape[1], math.ceil((data.shape[1]+width)/2))

        data[s0:e0, s1:e1].fill(0)

        data[:s0, s1:e1].fill(0)
        data[e0:, s1:e1].fill(0)
        data[s0:e0, :s1].fill(0)
        data[s0:e0, e1:].fill(0)

    def BlindOuter(self, data: np.array, height: int, width: int):
        s0 = max(0, math.floor((data.shape[0]-height)/2))
        s1 = max(0, math.floor((data.shape[1]-width)/2))
        e0 = min(data.shape[0], math.ceil((data.shape[0]+height)/2))
        e1 = min(data.shape[1], math.ceil((data.shape[1]+width)/2))

        data[:s0, :s1].fill(0)
        data[:s0, e1:].fill(0)
        data[e0:, :s1].fill(0)
        data[e0:, e1:].fill(0)

        data[:s0, s1:e1].fill(0)
        data[e0:, s1:e1].fill(0)
        data[s0:e0, :s1].fill(0)
        data[s0:e0, e1:].fill(0)

    def Copy(self, src: np.array, dest: np.array):
        n0 = math.ceil(min(src.shape[0], dest.shape[0])/2)
        n1 = math.ceil(min(src.shape[1], dest.shape[1])/2)
        dest[1-n0:n0-1, :n1-1].fill(0)
        dest[1-n0:n0-1, 1-n1:].fill(0)
        dest[:n0-1, 1-n1:n1-1].fill(0)
        dest[1-n0:, 1-n1:n1-1].fill(0)
        dest[1-n0:n0-1, 1-n1:n1-1].fill(0)
        dest[:n0, :n1] = src[:n0, :n1]
        dest[-n0:, :n1] = src[-n0:, :n1]
        dest[:n0, -n1:] = src[:n0, -n1:]
        dest[-n0:, -n1:] = src[-n0:, -n1:]

