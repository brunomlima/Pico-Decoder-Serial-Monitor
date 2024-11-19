#include <Arduino.h>
#include <Keyboard.h>

// Definição dos pinos do encoder e do botão
#define CLK_PIN 10  // Pino para CLK do encoder
#define DT_PIN 11   // Pino para DT do encoder
#define SW_PIN 12   // Pino para o botão do encoder

int clkLastState = HIGH;
unsigned long lastDebounceTime = 0;
const unsigned long debounceDelay = 5;
int functionKey = 0;  // Controla as teclas F no modo 3

unsigned long buttonPressStart = 0;  // Tempo em que o botão foi pressionado
bool longPressDetected = false;     // Flag para verificar se um pressionamento longo foi detectado
const unsigned long longPressDuration = 2000;  // Tempo para pressionamento longo (2 segundos)

// Modos de operação
enum Mode { TIMELINE, SPEED, VOLUME };
Mode currentMode = VOLUME;

void setup() {
    Serial.begin(115200);
    pinMode(CLK_PIN, INPUT);
    pinMode(DT_PIN, INPUT);
    pinMode(SW_PIN, INPUT_PULLUP);

    Keyboard.begin();
    delay(5000);  // Tempo para estabilização
    Serial.println("== Controle e Volume e Capcut Iniciado ==");
}

// Função para alternar entre os modos
void switchMode() {
    currentMode = static_cast<Mode>((currentMode + 1) % 3);
    functionKey = 0;  // Reseta a tecla F ao alternar para o modo 3
    String modeName = (currentMode == VOLUME) ? "Modo: Volume" :
                      (currentMode == TIMELINE) ? "Modo: Timeline - Ctrl + / Ctrl -" :
                      "Modo: Velocidade - J / L";
    Serial.println(modeName);
}

// Ação rápida para o modo TIMELINE
void quickPressTimeline() {
    Serial.println("Quick Press (TIMELINE - Dividir): Ctrl + B");
    Keyboard.press(KEY_LEFT_CTRL);
    Keyboard.write('b');
    Keyboard.releaseAll();
}

// Ação rápida para o modo SPEED
void quickPressSpeed() {
    Serial.println("Quick Press (SPEED - Dividir): Ctrl + B");
    Keyboard.press(KEY_LEFT_CTRL);
    Keyboard.write('b');
    Keyboard.releaseAll();
}

// Ação rápida para o modo VOLUME
void quickPressVolume() {
    Serial.println("mute");    
}

// Executa a ação rápida baseada no modo atual
void quickPressAction() {
    switch (currentMode) {
        case TIMELINE:
            quickPressTimeline();
            break;
        case SPEED:
            quickPressSpeed();
            break;
        case VOLUME:
            quickPressVolume();
            break;
    }
}

// Função para verificar o encoder e executar ações
void verificarEncoder() {
    int currentCLK = digitalRead(CLK_PIN);
    int currentDT = digitalRead(DT_PIN);

    if (currentCLK != clkLastState) {
        if (millis() - lastDebounceTime > debounceDelay) {
            lastDebounceTime = millis();

            if (currentCLK == LOW) {
                if (currentDT == LOW) {
                    // Gira anti-horário
                    switch (currentMode) {
                        case TIMELINE:
                            Keyboard.press(KEY_LEFT_CTRL);
                            Keyboard.write('-');
                            Keyboard.releaseAll();
                            Serial.println("Ctrl + - (Afastar)");
                            break;
                        case SPEED:
                            Keyboard.write('j');
                            Keyboard.releaseAll();
                            Serial.println("J (Diminui velocidade)");
                            break;
                        case VOLUME:
                            Serial.println("vol_down");
                            break;
                    }
                } else {
                    // Gira horário
                    switch (currentMode) {
                        case TIMELINE:
                            Keyboard.press(KEY_LEFT_CTRL);
                            Keyboard.write('+');
                            Keyboard.releaseAll();
                            Serial.println("Ctrl + + (Aproximar)");
                            break;
                        case SPEED:
                            Keyboard.write('l');
                            Keyboard.releaseAll();
                            Serial.println("L (Aumenta velocidade)");
                            break;
                        case VOLUME:
                            Serial.println("vol_up");                            
                            break;
                    }
                }
            }
        }
        clkLastState = currentCLK;
    }
}

// Função para verificar o botão e alternar entre modos ou executar ação rápida
void verificarBotao() {
    if (digitalRead(SW_PIN) == LOW) {  // Botão pressionado
        if (buttonPressStart == 0) {
            buttonPressStart = millis();  // Marca o tempo em que o botão foi pressionado
        }

        if (!longPressDetected && (millis() - buttonPressStart >= longPressDuration)) {
            longPressDetected = true;
            switchMode();  // Alterna o modo após 2 segundos de pressão
        }
    } else {  // Botão liberado
        if (buttonPressStart > 0) {  // Se o botão foi pressionado anteriormente
            if (!longPressDetected) {
                quickPressAction();  // Executa ação rápida se não for um pressionamento longo
            }
            buttonPressStart = 0;  // Reseta o tempo de pressão
            longPressDetected = false;  // Reseta a flag de pressionamento longo
        }
    }
}

// Função para verificar o comando de "ping" e responder com "pong"
void verificarComandoSerial() {
    if (Serial.available() > 0) {
        String command = Serial.readStringUntil('\n'); // Lê o comando recebido
        command.trim(); // Remove espaços em branco

        if (command == "ping") {
            Serial.println("pong"); // Responde com "pong" se receber "ping"
        }
    }
}

void loop() {
    verificarEncoder();
    verificarBotao();
    verificarComandoSerial();  // Verifica se há comando "ping" na serial
}
