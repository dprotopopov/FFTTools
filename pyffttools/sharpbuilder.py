from builderbase import BuilderBase
import numpy as np
import math

class SharpBuilder(BuilderBase):

    def __init__(self, height: int, width: int):
        self.__height__ = height
        self.__width__ = width

    def Sharp(self, data: np.array):
        average = np.average(data)
        variance = np.var(data)
        complexes = self.Forward(data)
        level = complexes.flat[0] 
        self.BlindOuter(complexes, self.__height__, self.__width__)
        complexes.flat[0] = level
        data2 = self.Backward(complexes)
        average2 = np.average(data2)
        variance2 = np.var(data2)
        a = math.sqrt(variance/variance2)
        b = average - average2*a
        return a*data2+b

    def ToBitmap(self, shape: np.array):
        data = np.ndarray(shape)
        data.fill(1)
        self.BlindOuter(data, self.__height__, self.__width__)
        return data
 
