using System;
using Reversi.Players;
using Reversi.Players.AI;
using Reversi.Players.Input;
using Reversi.Stones.Stone;
using VContainer;

namespace Reversi.Managers
{
    /// <summary>
    /// プレイヤー管理クラス
    /// </summary>
    public class PlayerManager
    {
        private readonly StoneManager _stoneManager;
        private readonly IInputEventProvider _inputEventProvider;

        /// <summary>
        /// プレイヤー
        /// </summary>
        private IPlayer _player1;
        private IPlayer _player2;

        /// <summary>
        /// アクティブプレイヤー
        /// </summary>
        private IPlayer _activePlayer;

        /// <summary>
        /// ゲーム終了処理
        /// </summary>
        private Action _changeNextTurnAction;

        [Inject]
        public PlayerManager(StoneManager stoneManager, IInputEventProvider inputEventProvider)
        {
            _stoneManager = stoneManager;
            _inputEventProvider = inputEventProvider;
        }

        /// <summary>
        /// ゲーム初期化
        /// </summary>
        /// <param name="player1Kind">プレイヤー1</param>
        /// <param name="player2Kind">プレイヤー2</param>
        /// <param name="changeNextTurnAction">ターン切り替え処理</param>
        public void InitializeGame(PlayerKind player1Kind, PlayerKind player2Kind, Action changeNextTurnAction)
        {
            _changeNextTurnAction = changeNextTurnAction;

            // ゲーム情報の設定
            _player1 = CreatePlayer(player1Kind, StoneState.White);
            _player2 = CreatePlayer(player2Kind, StoneState.Black);
            _activePlayer = _player1;
        }

        /// <summary>
        /// ターン開始処理
        /// </summary>
        public void StartTurn()
        {
            // 入力プレイヤーの場合、置けるストーンをフォーカスする
            if (_activePlayer.IsInputPlayer()) _stoneManager.SetFocusAllCanPutStones(_activePlayer.MyStoneState);
            // ターン開始
            _activePlayer.StartTurn(_stoneManager.GetStoneStatesClone());
        }

        /// <summary>
        /// ターン更新処理
        /// </summary>
        public void UpdateTurn()
        {
            // ターン更新
            _activePlayer.UpdateTurn();
        }

        /// <summary>
        /// プレイヤー切り替え
        /// </summary>
        /// <param name="turnCount"></param>
        public void ChangePlayer(int turnCount)
        {
            var isFirstTurn = turnCount % 2 == 0;
            _activePlayer = isFirstTurn ? _player1 : _player2;
        }

        /// <summary>
        /// アクティブプレイヤーがストーンを置けるかどうか？
        /// </summary>
        /// <returns></returns>
        public bool IsCanPutStoneActivePlayer()
        {
            return _stoneManager.GetAllCanPutStonesIndex(_activePlayer.MyStoneState).Count > 0;
        }

        /// <summary>
        /// ストーンを置く
        /// </summary>
        /// <param name="stoneState"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        private void PutStone(StoneState stoneState, int x, int z)
        {
            // ストーン置く
            if (_stoneManager.PutStone(stoneState, x, z))
            {
                // フォーカス解除
                _stoneManager.ReleaseFocusStones();

                // ターン切り替え
                _changeNextTurnAction();
            }
        }

        /// <summary>
        /// プレイヤー作成処理
        /// </summary>
        private IPlayer CreatePlayer(PlayerKind playerKind, StoneState stoneState)
        {
            switch (playerKind)
            {
                case PlayerKind.InputPlayer:
                    return new InputPlayer(stoneState, PutStone, _inputEventProvider);
                case PlayerKind.RandomAIPlayer:
                    return new RandomAIPlayer(stoneState, PutStone);
                case PlayerKind.MiniMaxAIPlayer:
                    return new MiniMaxAIPlayer(stoneState, PutStone);
            }
            return null;
        }
    }
}