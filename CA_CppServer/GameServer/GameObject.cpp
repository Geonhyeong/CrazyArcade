#include "pch.h"
#include "GameObject.h"

void GameObject::SetCellPos(int32 x, int32 y)
{
    Protocol::PositionInfo* posInfo = info.mutable_posinfo();
    posInfo->set_posx(x);
    posInfo->set_posy(y);
}

Vector2Int GameObject::GetFrontCellPos(Protocol::MoveDir dir, int32 distance)
{
    Vector2Int cellPos = GetCellPos();

    switch (dir)
    {
    case Protocol::MoveDir::UP:
        cellPos = cellPos + Vector2Int::up() * distance;
        break;
    case Protocol::MoveDir::DOWN:
        cellPos = cellPos + Vector2Int::down() * distance;
        break;
    case Protocol::MoveDir::LEFT:
        cellPos = cellPos + Vector2Int::left() * distance;
        break;
    case Protocol::MoveDir::RIGHT:
        cellPos = cellPos + Vector2Int::right() * distance;
        break;
    }

    return cellPos;

}