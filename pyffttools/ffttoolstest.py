import unittest
from builderbase import BuilderBase
import numpy as np


class TestBuilder(unittest.TestCase):

    def testBaseSplit(self):
        data = np.array([[[1, 1, 1], [2, 2, 2], [3, 3, 3]],
                         [[4, 4, 4], [5, 5, 5], [6, 6, 6]],
                         [[7, 7, 7], [8, 8, 8], [9, 9, 9]]])
        outer = np.empty(data.shape)
        middle = np.empty(data.shape)
        inner = np.empty(data.shape)
        builder = BuilderBase()
        builder.Split(data, outer, middle, inner, 1, 1)
        self.assertEqual(data[1, 1, 1], inner[1, 1, 1])
        self.assertEqual(data[0, 0, 0], outer[0, 0, 0])
        self.assertEqual(0, outer[1, 1, 1])
        self.assertEqual(0, inner[0, 0, 0])


if __name__ == '__main__':
    unittest.main()
