using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace Wordle
{
    class Wordle
    {
        private Dictionary<int, string>
            dicionario = new Dictionary<int, string>();
        
        private Random
            random = new Random();
        
        private int
            id = 1,
            numeroAleatorio,
            partida,
            vitoria,
            derrota;

        private char[]
            escolhidaComAcento = new char[5],
            escolhidaSemAcento = new char[5],
            letrasJogadaComAcento = new char[5],
            letrasJogadaSemAcento = new char[5],
            gabarito = new char[5];

        private string
            palavraJogada = "",
            menuEscolhido = "Menu Principal",
            modoDeJogo = "Start",
            resourcePath = "Wordle.arquivos.",
            linha;

        private bool
            contemAntes = false,
            contemDepois = false,
            validadorModoCustom = true;

        private List<char> letrasErradas = new List<char>();

        private List<string>
            palavrasPossiveis = new List<string>(),
            escolhidaLista = new List<string>(),
            palavrasJaJogadas = new List<string>(),
            consoantesRestantes = new List<string>(),
            vogaisRestantes = new List<string>(),
            validadorAmarelo = new List<string>();

        private DateTime
            data;
        
        private Assembly
            assembly = Assembly.GetExecutingAssembly();

        private Panel
            tutorial = new Panel("\nO jogo escolherá uma palavra e você terá [yellow]6 tentativas[/] para adivinhá-la.\nCada tentativa consiste na jogada de [blue]1 palavra[/].\n\n[red]ATENÇÃO[/], pois a palavra deve:\n\n* Existir no [blue]dicionário português brasileiro[/].\n* Conter [yellow]5 letras[/] e [yellow]APENAS letras[/].\n* Estar [green]corretamente acentuada[/].\n\nApós cada jogada:\n\n* Letras [green]corretas[/] na posição [green]correta[/] aparecerão em [green]verde[/].\n* Letras [green]corretas[/] na posição [red]incorreta[/] aparecerão em [yellow]amarelo[/].\n* Letras [red]incorretas[/] aparecerão em [red]vermelho[/].\n\nCaso suas tentativas se esgotem, você [red rapidblink]perde o jogo[/].\n\nExistem 3 modos de jogo disponíveis:\n\n* [blue]Clássico[/]: Uma palavra aleatória é escolhida.\n* [blue]Diário[/]: Cada dia uma palavra é escolhida.\n* [blue]Custom[/]: O jogador escolhe um número dentro de um intervalo, este número estará relacionada à uma palavra.")
            .Header("Como Jogar?").Border(BoxBorder.Rounded);

        public void menu()
        {
            if (!menuEscolhido.Equals("Pós Partida"))
            {
                limparTela();
                displayTitulo();
                Console.WriteLine();
            }

            switch (menuEscolhido)
            {
                case "Menu Principal":
                    {
                        modoDeJogo = "Start";

                        menuEscolhido = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .PageSize(10)
                        .AddChoices(new[] {
                            "Novo Jogo","Como Jogar?","Sair"
                        }));
                        break;
                    }
                case "Como Jogar?":
                    {
                        AnsiConsole.Write(tutorial);
                        Console.WriteLine();

                        menuEscolhido = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .PageSize(10)
                        .AddChoices(new[] {
                            "Novo Jogo","Menu Principal","Sair"
                        }));
                        break;
                    }
                case "Novo Jogo":
                    {
                        modoDeJogo = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .PageSize(10)
                        .AddChoices(new[] {
                            "Clássico","Diário","Custom"
                        }));
                        break;
                    }
                case "Pós Partida":
                    {
                        switch (modoDeJogo)
                        {
                            case "Clássico":
                                {
                                    menuEscolhido = AnsiConsole.Prompt(new SelectionPrompt<string>()
                                    .PageSize(10)
                                    .AddChoices(new[] {
                                        "Jogar Novamente","Menu Principal","Sair"
                                    }));
                                    break;
                                }
                            case "Diário":
                                {
                                    menuEscolhido = AnsiConsole.Prompt(new SelectionPrompt<string>()
                                    .PageSize(10)
                                    .AddChoices(new[] {
                                        "Menu Principal","Sair"
                                    }));
                                    break;
                                }
                            case "Custom":
                                {
                                    menuEscolhido = AnsiConsole.Prompt(new SelectionPrompt<string>()
                                    .PageSize(10)
                                    .AddChoices(new[] {
                                        "Jogar Novamente","Menu Principal","Sair"
                                    }));
                                    if (menuEscolhido.Equals("Jogar Novamente")) validadorModoCustom = true;
                                    break;
                                }
                        }
                        break;
                    }
            }
        }
        
        public string getModoDeJogo()
        {
            return modoDeJogo;
        }

        public string getMenuEscolhido()
        {
            return menuEscolhido;
        }
        
        public void popularDicioDeJogo() //Popula dicionário de palavras a serem sorteadas para adivinhação
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath + "159palavrasfiltradas.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                while ((linha = reader.ReadLine()) != null)
                {
                    dicionario.Add(id, linha.ToUpper());
                    id++;
                }
            }
        }

        public void popularDicioDePossiveisJogadas() //Popula dicionário de palavras possíveis a serem jogadas pelo jogador
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath + "palavrasMasSoCom5Letras.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                while ((linha = reader.ReadLine()) != null)
                {
                    palavrasPossiveis.Add(linha.ToUpper());
                }
            }
        }

        public void resetVitDerrPartidas()
        {
            partida = 0; vitoria = 0; derrota = 0;
        }

        public void limparTela()
        {
            Console.Clear();
            Console.WriteLine("\x1b[3J");
        }
        
        public void resetVarsParaNovaPartida()
        {
            letrasErradas.Clear();
            palavrasJaJogadas.Clear();
            letrasJogadaComAcento = new char[5];
            letrasJogadaSemAcento = new char[5];
            consoantesRestantes.Clear();
            vogaisRestantes.Clear();
            escolhidaLista.Clear();
            gabarito = new char[5];
            palavraJogada = "";
        }

        public void setup() //Configuração necessária pré inicio de partida.
        {
            switch (modoDeJogo)
            {
                case "Clássico":
                    {
                        partida++;
                        numeroAleatorio = random.Next(1, dicionario.Count);
                        break;
                    }
                case "Diário":
                    {
                        data = DateTime.Now.Date;
                        numeroAleatorio = ((((Math.Abs(data.Year - 2000)) + (data.DayOfYear * 11)) % (dicionario.Count - 1)) + 1);
                        break;
                    }
                case "Custom":
                    {
                        while (validadorModoCustom == true) {
                            Console.Write("\nEscolha um número inteiro entre 1 e {0}: ", dicionario.Count);
                            try
                            {
                                numeroAleatorio = Convert.ToInt32(Console.ReadLine());
                                if (numeroAleatorio.Equals("") || numeroAleatorio < 1)
                                {
                                    throw new ArithmeticException();
                                }
                                validadorModoCustom = false;
                            }
                            catch (Exception e)
                            {
                                textoCentralizado("\n[red]Número inválido.[/]\n\n* Deve ser um número.\n* Não pode estar vazio.\n* Deve estar entre 1 e "+dicionario.Count+".\n\nTente novamente!\n");
                            }
                        }
                        break;
                    }
            }
        }

        
        public void popularVogaisConsoantesRestantes() //Popula os dicionários de vogais e de consoantes restantes
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath + "consoantes.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                while ((linha = reader.ReadLine()) != null)
                {
                    consoantesRestantes.Add(linha);
                }
            }
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath + "vogais.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                while ((linha = reader.ReadLine()) != null)
                {
                    vogaisRestantes.Add(linha);
                }
            }
        }
        public void arrumarRandomizada() //Coloca a palavra escolhida em 2 variáveis (com/sem acento) para uso ao longo do código
        {
            escolhidaComAcento = dicionario[numeroAleatorio].ToCharArray();
            escolhidaSemAcento = removerAcentos(dicionario[numeroAleatorio]).ToCharArray();
            foreach (char letra in escolhidaSemAcento)
            {
                escolhidaLista.Add(letra.ToString());
            }
        }

        public void cheatDoDev() //código que mostra palavra usada, para checagem de funcionamento
        {
            Console.WriteLine(numeroAleatorio + ". " + dicionario[numeroAleatorio]);
            Console.Write("\nCom Acento: ");
            for (int i = 0; i < 5; i++)
            {
                Console.Write(escolhidaComAcento[i] + " ");
            }
            Console.Write("\nSem Acento: ");
            for (int i = 0; i < 5; i++)
            {
                Console.Write(escolhidaSemAcento[i] + " ");
            }
            Console.WriteLine();
        }

        public void displayTitulo()
        {
            textoCentralizado("-******-\nWORDLE\n-******-\n");
        }

        public void atualizarValidadorAmarelo()
            //atualizador da lista de caracteres da palavra a ser adivinhada
            //para que seja possivel checar multiplas ocorrencias da mesma letra
        {
            validadorAmarelo.Clear();
            foreach (char letra in escolhidaSemAcento)
            {
                validadorAmarelo.Add(letra.ToString());
            }
        }

        public void atualizarLetrasJogadas(string palavra)
            //similar ao arrumarRandomizada(), ele arruma as letras jogadas pelo jogador em duas variaveis com/sem acentos
        {
            letrasJogadaComAcento = palavra.ToCharArray();
            letrasJogadaSemAcento = removerAcentos(palavra).ToCharArray();
        }

        public void resetContemAmarelos() //reset das variaveis contém, usadas para checagem de letras amarelas
        {
            contemAntes = false;
            contemDepois = false;
        }

        public void removerConsoante(int contador) //remove consoantes jogadas do display de consoantes restantes
        {
            if (consoantesRestantes.Contains(letrasJogadaSemAcento[contador].ToString()) == true)
            {
                consoantesRestantes.Remove(letrasJogadaSemAcento[contador].ToString());
            }
        }

        public void removerVogal(int contador) //remove vogal jogada do display de vogais restantes
        {
            if (vogaisRestantes.Contains(letrasJogadaSemAcento[contador].ToString()) == true)
            {
                vogaisRestantes.Remove(letrasJogadaSemAcento[contador].ToString());
            }
        }

        public void checarAntes(int contador) //checa se existe a letra antes da posição de uma letra na palavra
        {
            for (int contadorCheck = 0; contadorCheck < contador; contadorCheck++)
            {
                if (letrasJogadaSemAcento[contador].ToString().Equals(escolhidaSemAcento[contadorCheck].ToString()))
                {
                    contemAntes = true;
                }
            }
        }

        public void checarDepois(int contador) //checa se existe a letra depois da posição de uma letra na palavra
        {
            for (int contadorCheck = contador + 1; contadorCheck < 5; contadorCheck++)
            {
                //checa se a letra existe
                if (letrasJogadaSemAcento[contador].ToString().Equals(escolhidaSemAcento[contadorCheck].ToString()))
                {
                    //checa se a letra existente é uma letra verde, se não for, então ele muda o contém
                    if (letrasJogadaSemAcento[contadorCheck].ToString().Equals(escolhidaSemAcento[contadorCheck].ToString()) == false)
                    {
                        contemDepois = true;
                    }
                }
            }
        }
        public string checarAmarelo(int contador) //checa letras amarelas
        {
            checarAntes(contador);
            checarDepois(contador);
            //se contem antes ou depois e o contém a letra no validadorAmarelo, ele remove a letra do validador para que seja possível
            //checar múltiplas ocorrências da mesma letra
            if ((contemAntes == true || contemDepois == true) && validadorAmarelo.Contains(letrasJogadaSemAcento[contador].ToString()) == true)
            {
                validadorAmarelo.Remove(letrasJogadaSemAcento[contador].ToString());
                return "[yellow]" + letrasJogadaComAcento[contador] + "[/]";
            }
            return checarVermelho(contador);
        }
        public string checarVermelho(int contador)
        {
            //adiciona a letra a lista de letras erradas
            if (letrasErradas.Contains(letrasJogadaSemAcento[contador]) == false && escolhidaLista.Contains(letrasJogadaSemAcento[contador].ToString()) == false)
            {
                letrasErradas.Add(letrasJogadaSemAcento[contador]);
            }
            return "[red]" + letrasJogadaComAcento[contador] + "[/]";
        }
        public string checarVerde(int contador)
        {
            if (letrasJogadaSemAcento[contador].ToString().Equals(escolhidaSemAcento[contador].ToString()))
            {
                //marca no gabarito
                gabarito[contador] = escolhidaComAcento[contador];
                //remover das opções de amarelo
                validadorAmarelo.Remove(letrasJogadaSemAcento[contador].ToString());
                return "[green]" + letrasJogadaComAcento[contador] + "[/]";
            }
            return checarAmarelo(contador);
        }

        public void asteriscoJogadasRestantes(int tentativas) //display visual das jogadas restantes
        {
            for (int restantes = tentativas; restantes > 0; restantes--)
            {
                textoCentralizado("* * * * *");
            }
        }

        public bool checarVitoria() //validador de vitória
        {
            if (removerAcentos(palavraJogada).Equals(removerAcentos(dicionario[numeroAleatorio])))
            {
                textoCentralizado("\n[rapidblink bold]FIM DE JOGO![/]\n");
                textoCentralizado("\n*-----------------------*\n[green]PARABÉNS![/] Você acertou!\n*-----------------------*\n");
                vitoria++;
                menuEscolhido = "Pós Partida";
                return true;
            }
            else return false;
        }

        public bool checarTentativas(int tentativas) //validador de tentativas restantes
        {
            if (tentativas == 0)
            {
                textoCentralizado("\n[rapidblink bold]FIM DE JOGO![/]\n");
                textoCentralizado("\n[red]Você perdeu![/] A palavra era [green]" + dicionario[numeroAleatorio] + "[/]");
                derrota++;
                menuEscolhido = "Pós Partida";
                return true;
            } else return false;
        }

        public void displayTentativas(int tentativas)
        {
            textoCentralizado("\nTentativas restantes: " + tentativas);
        }

        public void displayGabarito()
        {
            string gabaritoFormatado = "";

            for (int contador = 0; contador < 5; contador++)
            {
                if (gabarito[contador] != '\0')
                {
                    gabaritoFormatado = gabaritoFormatado + "[green]" + gabarito[contador] + "[/] ";
                }
                else
                {
                    gabaritoFormatado = gabaritoFormatado + "* ";
                }
            }
            textoCentralizado("Gabarito: \n" + gabaritoFormatado);
        }

        public void displayLetrasIncorretas()
        {
            textoCentralizado("\n- Letras Incorretas -");
            textoCentralizado("[red]" + string.Join(" ", letrasErradas) + "[/]");
            if (letrasErradas.Any() == false)
            {
                textoCentralizado("[red]!NENHUMA![/]");
            }
            Console.WriteLine();
        }

        public void textoCentralizado(string texto) { 
            AnsiConsole.Write(new Align(new Markup(texto), HorizontalAlignment.Center));
        }

        public void displayLetrasRestantes()
        {
            textoCentralizado("\n- Letras Restantes -");
            textoCentralizado("Consoantes:");
            textoCentralizado("[green]" + string.Join(" ", consoantesRestantes) + "[/]");
            if (consoantesRestantes.Any() == false)
            {
                textoCentralizado("!NENHUMA!");
            }
            textoCentralizado("Vogais:");
            textoCentralizado("[green]" + string.Join(" ", vogaisRestantes) + "[/]");
            if (vogaisRestantes.Any() == false)
            {
                textoCentralizado("[red]!NENHUMA![/]");
            }
        }

        public void digitarPalavra()
        {
            Console.Write("\nDigite uma palavra: ");
            palavraJogada = Console.ReadLine().ToUpper();
        }

        public void validarPalavraDigitada() //valida se a palavra digitada pelo jogador está de acordo com as regras
        {
            while (palavraJogada.Length != 5 || palavraJogada.Contains(' ') == true || palavraJogada.Any(char.IsDigit) == true || palavrasPossiveis.Contains(palavraJogada) == false)
            {
                textoCentralizado("[red]\nPALAVRA INVÁLIDA![/]\nA palavra deve:\n\n* Conter 5 letras *\n* Conter APENAS letras *\n* Existir no português brasileiro *\n* Estar acentuada corretamente *\n\nTente novamente.");
                digitarPalavra();
            }
        }

        public void realizarJogada() //adiciona palavra jogada à lista de palavras já jogadas
        {
            palavrasJaJogadas.Add(palavraJogada);
        }

        public void comparadorLxL() //comparador de letras da palavra jogada com as letras da palavra a ser adivinhada
        {
            string palavraFormatada = "";
            //aplica tabulação se for o inicio da palavra
            for (int comparador = 0; comparador < 5; comparador++)
            {
                resetContemAmarelos();

                removerConsoante(comparador);
                removerVogal(comparador);
                palavraFormatada = palavraFormatada + checarVerde(comparador)+" ";
            }
            AnsiConsole.Write(new Align(new Markup(palavraFormatada), HorizontalAlignment.Center));
        }

        public void displayJogadas() //mostra todas as jogadas na tela
        {
            foreach (string palavra in palavrasJaJogadas)
            {
                atualizarValidadorAmarelo();
                atualizarLetrasJogadas(palavra);
                comparadorLxL();
                Console.WriteLine();
            }
        }
        public void displayHeader()
        {
            switch (modoDeJogo)
            {
                case "Clássico":
                    {
                        textoCentralizado("-----------\nPARTIDA " + partida + "\n-----------\nVitórias: " + vitoria + "\nDerrotas: " + derrota + "\n");
                        break;
                    }
                case "Diário":
                    {
                        textoCentralizado("\n- DIA -\n" + data.Date.ToString("d") + "\n");
                        break;
                    }
                case "Custom":
                    {
                        textoCentralizado("\n- PALAVRA " + numeroAleatorio + "\n");
                        break;
                    }
            }
        }
        public void loopRodadas() //loop das rodadas dentro de uma partida
        {
            for (int tentativas = 6; tentativas >= 0; tentativas--)
            {
                limparTela();
                //ATIVADOR DE CÓDIGO QUE MOSTRA A PALAVRA SORTEADA
                //cheatDoDev();
                displayTitulo();
                displayHeader();
                displayJogadas();
                asteriscoJogadasRestantes(tentativas);

                if (checarVitoria() == true || checarTentativas(tentativas) == true) break;

                displayTentativas(tentativas);

                displayGabarito();
                displayLetrasIncorretas();
                displayLetrasRestantes();

                digitarPalavra();
                validarPalavraDigitada();
                realizarJogada();
            }
        }

        public void novoJogo() //setup para um novo jogo
        {
            resetVarsParaNovaPartida();
            setup();
            popularVogaisConsoantesRestantes();

            arrumarRandomizada();
            Console.WriteLine();

            loopRodadas();
        }

        public string removerAcentos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return texto;

            string normalizado = texto.Normalize(NormalizationForm.FormD);

            Regex regex = new Regex(@"[\p{Mn}]");
            return regex.Replace(normalizado, string.Empty)
                        .Normalize(NormalizationForm.FormC);
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Wordle wordle = new Wordle();

            wordle.popularDicioDeJogo();
            wordle.popularDicioDePossiveisJogadas();

            while(!wordle.getMenuEscolhido().Equals("Sair"))
            {
                wordle.resetVitDerrPartidas();
                wordle.menu();

                while (wordle.getMenuEscolhido().Equals("Jogar Novamente") || wordle.getMenuEscolhido().Equals("Novo Jogo") && !wordle.getModoDeJogo().Equals("Start"))
                {
                    wordle.novoJogo();
                }
            }
        }
    }
}
