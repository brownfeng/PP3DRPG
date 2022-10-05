
public interface IEndGameObserver
{
    // 游戏关闭时, 游戏关闭的全局通知, 针对所有需要接受游戏停止通知的组件, 都继承这个接口
    void EndNotify();
}
