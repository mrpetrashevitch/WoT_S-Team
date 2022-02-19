#pragma once
#include "../../WebDll/src/defs.h"

namespace ai
{
	struct point
	{
		int x = 0, y = 0, z = 0;

		bool operator==(point right) {
			if (x == right.x && y == right.y && z == right.z)
				return true;
			else
				return false;
		}
	};
}