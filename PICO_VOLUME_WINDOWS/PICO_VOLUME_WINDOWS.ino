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

// Modos de operação
enum Mode { UNDO_REDO, COPY_PASTE, FUNCTION_KEYS };
Mode currentMode = UNDO_REDO;

void setup() {
    Serial.begin(115200);
    pinMode(CLK_PIN, INPUT);
    pinMode(DT_PIN, INPUT);
    pinMode(SW_PIN, INPUT_PULLUP);

    Keyboard.begin();
    delay(1000);  // Tempo para estabilização
    Serial.println("== Controle de Teclas Iniciado ==");
}

// Função para alternar entre os modos
void switchMode() {
    currentMode = static_cast<Mode>((currentMode + 1) % 3);
    functionKey = 0;  // Reseta a tecla F ao alternar para o modo 3
    String modeName = (currentMode == UNDO_REDO) ? "Modo: Ctrl + Z / Ctrl + Y" :
                      (currentMode == COPY_PASTE) ? "Modo: Ctrl + C / Ctrl + V" : 
                      "Modo: Teclas F1 até F23";
    Serial.println(modeName);
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
                        case UNDO_REDO:
                            Keyboard.press(KEY_LEFT_CTRL);
                            Keyboard.write('z');
                            Keyboard.releaseAll();
                            Serial.println("Ctrl + Z (Undo)");
                            break;
                        case COPY_PASTE:
                            Keyboard.press(KEY_LEFT_CTRL);
                            Keyboard.write('c');
                            Keyboard.releaseAll();
                            Serial.println("Ctrl + C (Copy)");
                            break;
                        case FUNCTION_KEYS:
                            if (functionKey > 0) functionKey--;
                            Keyboard.write(KEY_F1 + functionKey);
                            Serial.print("F");
                            Serial.println(functionKey + 1);
                            break;
                    }
                } else {
                    // Gira horário
                    switch (currentMode) {
                        case UNDO_REDO:
                            Keyboard.press(KEY_LEFT_CTRL);
                            Keyboard.write('y');
                            Keyboard.releaseAll();
                            Serial.println("Ctrl + Y (Redo)");
                            break;
                        case COPY_PASTE:
                            Keyboard.press(KEY_LEFT_CTRL);
                            Keyboard.write('v');
                            Keyboard.releaseAll();
                            Serial.println("Ctrl + V (Paste)");
                            break;
                        case FUNCTION_KEYS:
                            if (functionKey < 22) functionKey++;
                            Keyboard.write(KEY_F1 + functionKey);
                            Serial.print("F");
                            Serial.println(functionKey + 1);
                            break;
                    }
                }
            }
        }
        clkLastState = currentCLK;
    }
}

// Função para verificar o botão e alternar entre modos
void verificarBotao() {
    if (digitalRead(SW_PIN) == LOW) {  // Botão pressionado
        delay(500); // Evita rebotes
        if (digitalRead(SW_PIN) == LOW) {  // Se ainda estiver pressionado
            switchMode();  // Alterna o modo após 1 segundo de pressão
        }
        while (digitalRead(SW_PIN) == LOW);  // Espera até o botão ser solto
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
