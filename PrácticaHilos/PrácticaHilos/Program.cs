using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrácticaHilos
{
    internal class Program
    {
        static Queue<Pedido> PedidosPendientes = new Queue<Pedido>();
        static Queue<Pedido> PedidosListos = new Queue<Pedido>();
        static object lockObject = new object();

        static void Main(string[] args)
        {

            // Crear hilos para clientes, cocineros y empleados de entrega
            Thread clienteThread = new Thread(() => GenerarPedidos());
            Thread cocineroThread = new Thread(() => PrepararPedidos());
            Thread entregaThread = new Thread(() => EntregarPedidos());

            // Iniciar los hilos
            clienteThread.Start();
            cocineroThread.Start();
            entregaThread.Start();

            // Esperar a que los hilos terminen
            clienteThread.Join();
            cocineroThread.Join();
            entregaThread.Join();

            Console.WriteLine("Simulación completa.");

        }

        static void GenerarPedidos()
        {
            Random random = new Random();

            for (int i = 0; i < 10; i++) // Generar 10 pedidos
            {
                List<string> platos = new List<string> { "Plato1", "Plato2", "Plato3" };
                List<string> bebidas = new List<string> { "Bebida1", "Bebida2" };
                Pedido pedido = new Pedido(i, platos[random.Next(platos.Count)], bebidas[random.Next(bebidas.Count)]);

                lock (lockObject)
                {
                    PedidosPendientes.Enqueue(pedido);
                    Console.WriteLine($"Cliente {pedido.ClienteID} ha realizado un pedido: {pedido.Plato}, {pedido.Bebida}");
                    Monitor.Pulse(lockObject); // Notificar a los cocineros
                }

                Thread.Sleep(1000); // Esperar 1 segundo antes de generar otro pedido
            }
        }

        static void PrepararPedidos()
        {
            while (true)
            {
                Pedido pedido;

                lock (lockObject)
                {
                    while (PedidosPendientes.Count == 0)
                        Monitor.Wait(lockObject); // Esperar hasta que haya pedidos pendientes

                    pedido = PedidosPendientes.Dequeue();
                    Console.WriteLine($"Cocinero ha preparado el pedido: {pedido.Plato}, {pedido.Bebida}");
                    PedidosListos.Enqueue(pedido);
                }

                Thread.Sleep(2000); // Simular tiempo de preparación
            }
        }

        static void EntregarPedidos()
        {
            while (true)
            {
                Pedido pedido;

                lock (lockObject)
                {
                    while (PedidosListos.Count == 0)
                        Monitor.Wait(lockObject); // Esperar hasta que haya pedidos listos

                    pedido = PedidosListos.Dequeue();
                    Console.WriteLine($"Empleado de entrega ha entregado el pedido: {pedido.Plato}, {pedido.Bebida}");
                }

                Thread.Sleep(1000); // Simular tiempo de entrega
            }
        }
    }

    class Pedido
    {
        public int ClienteID { get; }
        public string Plato { get; }
        public string Bebida { get; }

        public Pedido(int clienteID, string plato, string bebida)
        {
            ClienteID = clienteID;
            Plato = plato;
            Bebida = bebida;
        }
    }

}
