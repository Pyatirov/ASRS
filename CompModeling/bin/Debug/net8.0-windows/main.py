import json
import sys
import numpy as np
from scipy.optimize import least_squares


def residuals(x, K, b):
    x1, x2, x3, x4, x5 = x
    r = np.zeros(5)

    # Уравнение 1
    r[0] = b[0] - (x1 + K[7] * x1 * x2 + K[12] * x1 * x2 ** 3 * x4 ** 3 + K[13] * x1 * x2 ** 3 * x4 ** 4)

    # Уравнение 2
    r[1] = b[1] - (x2 + K[5] * x2 * x3 + K[7] * x1 * x2 + K[8] * x2 * x4 ** 2 * x5
                   + 3 * K[9] * x2 ** 3 * x3 ** 3 * x4 + K[10] * x2 * x3 * x4
                   + 2 * K[11] * x2 ** 2 * x3 ** 2 * x4 + 3 * K[12] * x1 * x2 ** 3 * x4 ** 3
                   + 3 * K[13] * x1 * x2 ** 3 * x4 ** 4)

    # Уравнение 3
    r[2] = b[2] - (x3 + K[5] * x2 * x3 + K[9] * x2 ** 3 * x3 ** 3 * x4
                   + K[10] * x2 * x3 * x4 + 2 * K[11] * x2 ** 2 * x3 ** 2 * x4)

    # Уравнение 4
    r[3] = b[3] - (x4 + 2 * K[8] * x2 * x4 ** 2 * x5 + K[9] * x2 ** 3 * x3 ** 3 * x4
                   + K[10] * x2 * x3 * x4 + K[11] * x2 ** 2 * x3 ** 2 * x4
                   + 3 * K[12] * x1 * x2 ** 3 * x4 ** 3 + 4 * K[13] * x1 * x2 ** 3 * x4 ** 4
                   + 2 * K[14] * x4 ** 2)

    # Уравнение 5
    r[4] = b[4] - (x5 + K[8] * x2 * x4 ** 2 * x5)

    return r


if __name__ == "__main__":
    # Чтение входных данных из C#
    input_data = json.load(sys.stdin)

    # Извлечение параметров
    x_initial = input_data["x"]
    K = input_data["K"]
    b = input_data["b"]

    # Решение методом Левенберга-Марквардта
    result = least_squares(
        fun=lambda x: residuals(x, K, b),
        x0=x_initial,
        method='lm',
        ftol=1e-6,
        xtol=1e-6
    )

    # Возврат результатов в C#
    output = {
        "x": result.x.tolist(),
        "residuals": residuals(result.x, K, b).tolist()
    }
    print(json.dumps(output))