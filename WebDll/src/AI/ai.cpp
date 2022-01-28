#include "ai.h"

namespace ai
{
    action ai::get_action(int curr_player, 
        Player_native* players, int players_size, 
        Vehicle_native* vehicle, int vehicle_size, 
        WinPoints_native* win_points, int win_points_size, 
        AttackMatrix_native* attack_matrix, int attack_matrix_size)
    {
        action act;
        act.action_type = action_type::move;
        act.point = { 10,20,30 };
        act.vec_id = 100;
        return act;
    }
}
