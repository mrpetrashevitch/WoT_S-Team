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
				vehicles_native** targets = new vehicles_native*[vehicles_size];
				int targets_size = 0;
				get_targets(curr_player, vehicles, vehicles_size, vehicles[j].position, targets, &targets_size);
				bool is_shoot_ok = false;
				for (int k = 0; k < targets_size; k++) {
					if (check_neutrality(curr_player, targets[k]->player_id, attack_matrix, attack_matrix_size)) {
						is_shoot_ok = true;
						out_actions->actions[i].action_type = action_type::shoot;
						out_actions->actions[i].vec_id = vehicles[j].vehicle_id;
						out_actions->actions[i].point = targets[k] -> position;
						i++;
						break;
					}
				}
				if (is_shoot_ok) {
					continue;
				}
				out_actions->actions[i].action_type = action_type::move;
				out_actions->actions[i].vec_id = vehicles[j].vehicle_id;
				out_actions->actions[i].point = vehicles[j].position;
				if (base->x < vehicles[j].position.x) {
					out_actions->actions[i].point.x -= 2;
					out_actions->actions[i].point.y++;
					out_actions->actions[i].point.z++;
				}
				else if (base->y < vehicles[j].position.y) {
					out_actions->actions[i].point.x++;
					out_actions->actions[i].point.y -= 2;
					out_actions->actions[i].point.z++;
				}
				else if (base->z < vehicles[j].position.z) {
					out_actions->actions[i].point.x++;
					out_actions->actions[i].point.y++;
					out_actions->actions[i].point.z -= 2;
				}
				i++;
			}
		return Result::OKEY;
	}

	void ai::get_targets(int curr_player,
		vehicles_native* vehicles, int vehicles_size,
		point position,
		vehicles_native** targets, int* targets_size)
	{

		int i = 0;
		for (int j = 0; j < vehicles_size; j++) {
			if (vehicles[j].player_id != curr_player && distance(position, vehicles[j].position) == 2
				&& vehicles[j].health != 0) {
				targets[i] = &vehicles[j];
				i++;
			}
		}
		std::sort(targets, targets + i, [](vehicles_native* l, vehicles_native* r) {return l->health < r->health; });
		*targets_size = i;

	}

	int ai::distance(point a, point b)
	{
		return (abs(a.x - b.x) + abs(a.y - b.y) + abs(a.z - b.z)) / 2;
	}

	bool ai::check_neutrality(int curr_player, int goal,
		AttackMatrix_native* attack_matrix, int attack_matrix_size)
	{
		return true;
	}
}
