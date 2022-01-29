#include "ai.h"


namespace ai
{
	Result ai::get_action(int curr_player,
		player_native* players, int players_size,
		vehicles_native* vehicles, int vehicles_size,
		win_points_native* win_points, int win_points_size,
		AttackMatrix_native* attack_matrix, int attack_matrix_size,
		point* base, int base_size, action_ret* out_actions)
	{
	
			for (int j = 0, i = 0; j < vehicles_size; j++)
			{
				/*if (vehicles[j].player_id == curr_player)
				{
					out_actions->actions[i].action_type = action_type::move;
					out_actions->actions[i].vec_id = vehicles[j].vehicle_id;
					out_actions->actions[i].point = vehicles[j].position;
					out_actions->actions[i].point.x++;
					out_actions->actions[i].point.z--;
					i++;
				}*/
				if (vehicles[j].player_id != curr_player) {
					continue;
				}
				vehicles_native* targets;
				int targets_size = 0;
				get_targets(curr_player, vehicles, vehicles_size, vehicles[j].position, targets, &targets_size);
				bool is_shoot_ok = false;
				for (int k = 0; k < targets_size; k++) {

				}

			}
		return Result::OKEY;
	}

	void ai::get_targets(int curr_player,
		vehicles_native* vehicles, int vehicles_size,
		point position,
		vehicles_native* targets, int* targets_size)
	{

		int i = 0;
		for (int j = 0; j < vehicles_size; j++) {
			if (vehicles[j].player_id != curr_player && distance(position, vehicles[j].position) == 2) {
				targets[i] = vehicles[j];
				i++;
			}
		}
		std::sort(targets, targets + i, [](vehicles_native l, vehicles_native r) {return l.health < r.health; });
		*targets_size = i;

	}

	int ai::distance(point a, point b)
	{
		return (abs(a.x - b.x) + abs(a.y - b.y) + abs(a.z - b.z)) / 2;
	}
}
