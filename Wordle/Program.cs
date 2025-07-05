using Spectre.Console;
using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Wordle
{
    class Wordle
    {
        Dictionary<int, string> dicionario = new Dictionary<int, string>();
        
        Random random = new Random();
        
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
            menu = "Menu Principal",
            modoDeJogo = "Start",
            resourcePath = "Wordle.arquivos.",
            linha;

        private bool
            contemAntes = false,
            contemDepois = false,
            validador = true;

        List<char> letrasErradas = new List<char>();

        List<string>
            palavrasPossiveis = new List<string>(),
            escolhidaLista = new List<string>(),
            palavrasJaJogadas = new List<string>(),
            consoantesRestantes = new List<string>(),
            vogaisRestantes = new List<string>(),
            validadorAmarelo = new List<string>();

        DateTime data;
        
        Assembly assembly = Assembly.GetExecutingAssembly();

        public void menuPrincipal()
        {
            limparTela();
            displayTitulo();
            Console.WriteLine();

            menu = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .PageSize(10)
            .AddChoices(new[] {
                "Novo Jogo","Sair"
            }));
        }
        public void menuModosDeJogo()
        {
            limparTela();
            displayTitulo();
            Console.WriteLine();

            modoDeJogo = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .PageSize(10)
            .AddChoices(new[] {
            "Clássico","Diário", "Custom"
            }));
        }

        public void MenuPosPartida()
        {
            switch (modoDeJogo)
            {
                case "Clássico":
                    {
                        menu = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .PageSize(10)
                        .AddChoices(new[] {
                        "Jogar Novamente","Menu Principal","Sair"
                        }));
                        break;
                    }
                case "Diário":
                    {
                        menu = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .PageSize(10)
                        .AddChoices(new[] {
                        "Menu Principal","Sair"
                        }));
                        break;
                    }
                case "Custom":
                    {
                        menu = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .PageSize(10)
                        .AddChoices(new[] {
                        "Jogar Novamente","Menu Principal","Sair"
                        }));
                        break;
                    }
            }
        }
        public string getModoDeJogo()
        {
            return modoDeJogo;
        }

        public string getMenu()
        {
            return menu;
        }
        public void popularDicioDeJogo()
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

        public void popularDicioDePossiveisJogadas()
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
        }

        public void setup()
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
                        while (validador == true) {
                            Console.Write("\nEscolha um número inteiro entre 1 e {0}: ", dicionario.Count);
                            try
                            {
                                numeroAleatorio = Convert.ToInt32(Console.ReadLine());
                                if (numeroAleatorio.Equals("") || numeroAleatorio < 1)
                                {
                                    throw new ArithmeticException();
                                }
                                validador = false;
                            }
                            catch (Exception e)
                            {
                                AnsiConsole.Markup("\n[red]Número inválido.[/]\n\n* Deve ser um número.\n* Não pode estar vazio.\n* Deve estar entre 1 e {0}.\n\nTente novamente!\n", dicionario.Count);
                            }
                        }
                        break;
                    }
            }
        }

        
        public void popularVogaisConsoantesRestantes()
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
        public void arrumarRandomizada()
        {
            escolhidaComAcento = dicionario[numeroAleatorio].ToCharArray();
            escolhidaSemAcento = removerAcentos(dicionario[numeroAleatorio]).ToCharArray();
            foreach (char letra in escolhidaSemAcento)
            {
                escolhidaLista.Add(letra.ToString());
            }
        }

        public void cheatDoDev()
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
            Console.Write("\t -******-\n\t  WORDLE\n\t -******-\n");
        }

        public void atualizarValidadorAmarelo()
        {
            validadorAmarelo.Clear();
            foreach (char letra in escolhidaSemAcento)
            {
                validadorAmarelo.Add(letra.ToString());
            }
        }

        public void atualizarLetrasJogadas(string palavra)
        {
            letrasJogadaComAcento = palavra.ToCharArray();
            letrasJogadaSemAcento = removerAcentos(palavra).ToCharArray();
        }

        public void resetContemAmarelos()
        {
            contemAntes = false;
            contemDepois = false;
        }

        public void removerConsoante(int contador)
        {
            if (consoantesRestantes.Contains(letrasJogadaSemAcento[contador].ToString()) == true)
            {
                consoantesRestantes.Remove(letrasJogadaSemAcento[contador].ToString());
            }
        }

        public void removerVogal(int contador)
        {
            if (vogaisRestantes.Contains(letrasJogadaSemAcento[contador].ToString()) == true)
            {
                vogaisRestantes.Remove(letrasJogadaSemAcento[contador].ToString());
            }
        }

        public void checarAntes(int contador)
        {
            for (int contadorCheck = 0; contadorCheck < contador; contadorCheck++)
            {
                if (letrasJogadaSemAcento[contador].ToString().Equals(escolhidaSemAcento[contadorCheck].ToString()))
                {
                    contemAntes = true;
                }
            }
        }

        public void checarDepois(int contador)
        {
            for (int contadorCheck = contador + 1; contadorCheck < 5; contadorCheck++)
            {
                if (letrasJogadaSemAcento[contador].ToString().Equals(escolhidaSemAcento[contadorCheck].ToString()))
                {
                    if (letrasJogadaSemAcento[contadorCheck].ToString().Equals(escolhidaSemAcento[contadorCheck].ToString()) == false)
                    {
                        contemDepois = true;
                    }
                }
            }
        }
        public void checarAmarelo(int contador)
        {
            checarAntes(contador);
            checarDepois(contador);
            if ((contemAntes == true || contemDepois == true) && validadorAmarelo.Contains(letrasJogadaSemAcento[contador].ToString()) == true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(letrasJogadaComAcento[contador] + " ");
                Console.ResetColor();
                validadorAmarelo.Remove(letrasJogadaSemAcento[contador].ToString());
            }
            else
            {
                checarVermelho(contador);
            }
        }
        public void checarVermelho(int contador)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(letrasJogadaComAcento[contador] + " ");
            Console.ResetColor();
            //adiciona a letra a lista de letras erradas
            if (letrasErradas.Contains(letrasJogadaSemAcento[contador]) == false && escolhidaLista.Contains(letrasJogadaSemAcento[contador].ToString()) == false)
            {
                letrasErradas.Add(letrasJogadaSemAcento[contador]);
            }
        }
        public void checarVerde(int contador)
        {
            if (letrasJogadaSemAcento[contador].ToString().Equals(escolhidaSemAcento[contador].ToString()))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(letrasJogadaComAcento[contador] + " ");
                Console.ResetColor();
                //marca no gabarito
                gabarito[contador] = escolhidaComAcento[contador];
                //remover das opções de amarelo
                validadorAmarelo.Remove(letrasJogadaSemAcento[contador].ToString());

            }
            else
            {
                checarAmarelo(contador);
            }
        }

        public void asteJogadasRestantes(int tentativas)
        {
            for (int restantes = tentativas; restantes > 0; restantes--)
            {
                Console.WriteLine("\t* * * * *");
            }
        }

        public bool checarVitoria()
        {
            if (removerAcentos(palavraJogada).Equals(removerAcentos(dicionario[numeroAleatorio])))
            {
                AnsiConsole.Markup("\n\t[rapidblink bold]FIM DE JOGO![/]\n");
                Console.Write("\n  *-----------------------*\n");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("   PARABÉNS! ");
                Console.ResetColor();
                Console.Write("Você acertou!\n  *-----------------------*\n\n");
                vitoria++;
                return true;
            }
            else return false;
        }

        public bool checarTentativas(int tentativas)
        {
            if (tentativas == 0)
            {
                AnsiConsole.Markup("\n\t[rapidblink bold]FIM DE JOGO![/]\n");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\nVocê perdeu! ");
                Console.ResetColor();
                Console.Write("A palavra era ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(dicionario[numeroAleatorio]);
                Console.ResetColor();
                Console.WriteLine(".\n");
                derrota++;
                return true;
            } else return false;
        }

        public void displayTentativas(int tentativas)
        {
            Console.WriteLine("\nTentativas restantes: " + tentativas);
        }

        public void displayGabarito()
        {
            Console.Write("Gabarito: ");
            for (int contador = 0; contador < 5; contador++)
            {
                if (gabarito[contador] != '\0')
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(gabarito[contador] + " ");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("* ");
                }
            }
        }

        public void displayLetrasIncorretas()
        {
            Console.Write("\n\n\t - Letras incorretas -\n");
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (char letraErrada in letrasErradas)
            {
                Console.Write(letraErrada + " ");
            }
            if (letrasErradas.Any() == false)
            {
                Console.Write("!NENHUMA!");
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        public void displayLetrasRestantes()
        {
            Console.Write("\n\t - Letras restantes - \n");
            Console.Write("Consoantes: ");
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (string consoantesRestante in consoantesRestantes)
            {
                Console.Write(consoantesRestante + " ");
            }
            Console.ResetColor();
            Console.Write("\nVogais: ");
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (string vogaisRestante in vogaisRestantes)
            {
                Console.Write(vogaisRestante + " ");
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        public void digitarPalavra()
        {
            Console.Write("\nDigite uma palavra: ");
            palavraJogada = Console.ReadLine().ToUpper();
        }

        public void validarPalavraDigitada()
        {
            while (palavraJogada.Length != 5 || palavraJogada.Contains(' ') == true || palavraJogada.Any(char.IsDigit) == true || palavrasPossiveis.Contains(palavraJogada) == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nPALAVRA INVÁLIDA!");
                Console.ResetColor();
                Console.WriteLine("A palavra deve:\n* Conter 5 letras.\n* Conter apenas letras. (Não conter números ou espaço)\n* Ser uma palavra existente na língua portuguesa.\n* As acentuações devem estar corretas.\n\nTente novamente.");
                digitarPalavra();
            }
        }

        public void realizarJogada()
        {
            palavrasJaJogadas.Add(palavraJogada);
        }

        public void comparadorLxL()
        {
            for (int comparador = 0; comparador < 5; comparador++)
            {
                if (comparador == 0)
                {
                    Console.Write("\t");
                }

                resetContemAmarelos();

                removerConsoante(comparador);
                removerVogal(comparador);
                checarVerde(comparador);
            }
        }

        public void displayJogadas()
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
                        Console.WriteLine("\t-----------\n\t PARTIDA {0}\n\t-----------\nVitórias: {1}\tDerrotas: {2}\n", partida, vitoria, derrota);
                        break;
                    }
                case "Diário":
                    {
                        Console.Write("\n\t - DIA - \n\t{0:d}\n\n", data);
                        break;
                    }
                case "Custom":
                    {
                        Console.Write("\n       - PALAVRA {0} -\n\n", numeroAleatorio);
                        break;
                    }
            }
        }
        public void loopRodadas()
        {
            for (int tentativas = 6; tentativas >= 0; tentativas--)
            {
                limparTela();
                //cheatDoDev();
                displayTitulo();
                displayHeader();
                displayJogadas();
                asteJogadasRestantes(tentativas);

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

        public void novoJogo()
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
            if (string.IsNullOrWhiteSpace(texto))
                return texto;

            var normalizado = texto.Normalize(NormalizationForm.FormD);

            var regex = new Regex(@"[\p{Mn}]");
            return regex.Replace(normalizado, string.Empty)
                        .Normalize(NormalizationForm.FormC);
        }
        public void modoCustom()
        {

        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Wordle wordle = new Wordle();

            wordle.popularDicioDeJogo();
            wordle.popularDicioDePossiveisJogadas();

            while(wordle.getMenu().Equals("Menu Principal"))
            {
                wordle.resetVitDerrPartidas();
                wordle.menuPrincipal();

                if(wordle.getMenu().Equals("Novo Jogo"))
                {
                    wordle.menuModosDeJogo();
                }

                while (wordle.getMenu().Equals("Jogar Novamente") || wordle.getMenu().Equals("Novo Jogo"))
                {
                    wordle.novoJogo();

                    wordle.MenuPosPartida();
                }
            }
        }
    }
}
