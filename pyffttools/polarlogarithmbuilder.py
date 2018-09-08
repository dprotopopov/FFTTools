from builderbase import BuilderBase
import numpy as np
import math


class PolarLogarithmBuilder(BuilderBase):

    def __init__(self, height: int, width: int, a: float, b: float):
        self.__height__ = height
        self.__width__ = width
        uno = np.zeros(width)
        uno[1] = 1
        self.__cossin__ = self.Forward(uno)
        self.__radius__ = np.power(a, np.arange(height)-b)

    def Center(self, data: np.array):
        assert len(data.shape) >= 2
        return (data.shape[0]-1)/2, (data.shape[1]-1)/2

    def PolarLogarithm(self, data: np.array, y0: float, x0: float):
        assert len(data.shape) >= 2
        cossin = self.__cossin__
        radius = self.__radius__
        height = self.__height__
        width = self.__width__
        shape2 = np.copy(data.shape)
        shape2[0] = height
        shape2[1] = width
        height1 = data.shape[0]
        width1 = data.shape[1]
        data2 = np.ndarray(shape2, data.dtype)
        p0 = complex(x0, y0)
        for lr in range(height):
            r = radius[lr]
            for w in range(width):
                p = p0 + r*cossin[w]
                x = int(p.real)
                y = int(p.imag)
                data2[lr, w] = data[y, x] if (0 <= x < width1 and 0 <= y < height1) else 0
        return data2
