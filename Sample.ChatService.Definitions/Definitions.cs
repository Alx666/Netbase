using Netbase.Shared;

//Todo: overload delle operazioni non supportato
//Todo: operazioni con lo stesso nome nello stesso assembly non supportate, creare un namespace nested x ogni coppia di servizi
//Todo: task di prova
//Todo: task di prova2
namespace Sample.ChatService.Definitions
{
    [ServiceContract("ChatService", typeof(IChatSession))]
    public interface IChatService
    {
        [ServiceOperation]
        int Login(string sUsername, string sPassword);

        [ServiceOperation]
        void Logout();

        [ServiceOperation(RpcType.OneWay)]
        void Message(string sMessage);

        [ServiceOperation]
        Vector3 GetVector(VeryComplexType hComplex);
    }

    //Todo: task di prova2
    //Todo: task di prova2
    //Todo: task di prova2
    [CallbackContract("ChatSession", typeof(IChatService))]
    public interface IChatSession
    {
        //isjhrhgerdiohjrieuhg
        [ServiceOperation(RpcType.OneWay)]
        void ForwardMessage(string sSender, string sMessage);
    }


    [DataContract]
    public class VeryComplexType
    {
        public ComplexType hData;
    }

    [DataContract]
    public class ComplexType
    {
        public float somefloat;
    }

    [DataContract]
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }

}
