#include <Arduino.h>

// Definição dos pinos
#define CLK_PIN 10  // Pino para CLK do encoder (Amarelo)
#define DT_PIN 11   // Pino para DT do encoder (Laranja)
#define SW_PIN 12   // Pino para o botão do encoder (Marrom)

// Variáveis para estado do encoder
int clkLastState = HIGH;
bool processado = false;
unsigned long lastDebounceTime = 0;
const unsigned long debounceDelay = 5;

// Variáveis para estado do botão
bool botaoEstadoAnterior = HIGH;  // Estado anterior do botão (HIGH = não pressionado)

// Função de configuração inicial
void setup() {
    Serial.begin(115200);  // Inicializa a Serial para debug
    pinMode(CLK_PIN, INPUT);
    pinMode(DT_PIN, INPUT);
    pinMode(SW_PIN, INPUT_PULLUP);  // Ativa o pull-up interno no botão

    delay(5000);  // Tempo para estabilização (caso necessário)
    Serial.println("== Teste de Encoder Iniciado ==");
}

// Função para detectar direção do encoder
void verificarEncoder() {
    int currentCLK = digitalRead(CLK_PIN);
    int currentDT = digitalRead(DT_PIN);

    if (currentCLK != clkLastState) {  // Detecta mudança no CLK
        if (millis() - lastDebounceTime > debounceDelay) {  // Debounce
            if (currentCLK == LOW) {  // Borda de descida detectada
                if (currentDT == LOW) {
                    Serial.println("vol_up");
                } else {
                    Serial.println("vol_down");
                }
            }
            lastDebounceTime = millis();  // Atualiza o tempo do debounce
        }
        clkLastState = currentCLK;  // Atualiza o estado anterior do CLK
    }
}

// Função para verificar o botão com debounce
void verificarBotao() {
    bool botaoEstadoAtual = digitalRead(SW_PIN);  // Lê o estado atual do botão

    // Verifica se houve mudança no estado do botão
    if (botaoEstadoAtual != botaoEstadoAnterior) {
        if (millis() - lastDebounceTime > debounceDelay) {  // Debounce
            lastDebounceTime = millis();  // Atualiza o tempo do debounce

            // Se o botão foi pressionado (estado LOW)
            if (botaoEstadoAtual == LOW) {
                Serial.println("mute");
            }
        }
    }

    // Atualiza o estado anterior do botão
    botaoEstadoAnterior = botaoEstadoAtual;
}

void loop() {
    verificarEncoder();  // Verifica mudanças no encoder
    verificarBotao();    // Verifica o estado do botão
}
