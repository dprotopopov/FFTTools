from builderbase import BuilderBase
import numpy as np
import math

class StretchBuilder(BuilderBase):

    def __init__(self, height: int, width: int):
        self.__height__ = height
        self.__width__ = width

    def Stretch(self, data: np.array):
        average = np.average(data)
        variance = np.var(data)
        complexes = self.Forward(data)
        shape2 = np.copy(data.shape)
        shape2[0] = self.__height__
        shape2[1] = self.__width__
        complexes2 = np.ndarray(shape2, complex)
        self.Copy(complexes, complexes2)
        data2 = self.Backward(complexes2)
        average2 = np.average(data2)
        variance2 = np.var(data2)
        a = math.sqrt(variance/variance2)
        b = average - average2*a
        return (a*data2+b).astype(data.dtype)
