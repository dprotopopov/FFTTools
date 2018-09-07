from builderbase import BuilderBase
import numpy as np
import math


class CatchBuilder(BuilderBase):

    def __init__(self, pattern: np.array):
        self.__pattern__ = pattern

    def Catch(self, data: np.array):
        pattern = np.ndarray(data.shape)
        n0 = min(data.shape[0], self.__pattern__.shape[0])
        n1 = min(data.shape[1], self.__pattern__.shape[1])
        pattern[:n0, :n1] = self.__pattern__[:n0, :n1]
        pattern[n0:, :n1].fill(0)
        pattern[:n0, n1:].fill(0)
        pattern[n0:, n1:].fill(0)
        first = self.Forward(pattern)
        second = self.Forward(data)
        data1 = self.Backward(np.multiply(np.conjugate(first), second))[:, :, 0]
        pattern[:n0, :n1].fill(1)
        first = self.Forward(pattern)
        data2 = self.Backward(np.multiply(np.conjugate(np.multiply(np.conjugate(first), second)), second))[:, :, 0]
        return np.divide(data1+1, data2+1)
