/* Kernel includes. */
#include "FreeRTOS.h"
#include "task.h"
#include "timers.h"
#include "semphr.h"




// /* Hardware and starter kit includes. */
#include "stm32f4xx.h"
#include "stm32f4xx_conf.h"
#include "stm32f4_discovery.h"

float temp;
uint16_t adcValue;
ADC_InitTypeDef adc;

#define mainTIME_TASK_PRIORITY      ( tskIDLE_PRIORITY + 4 )



GPIO_InitTypeDef  GPIO_InitStructure;

static void vLED4( void *pvParameters );
static void vLED3( void *pvParameters );
static void vadc( void *pvParameters );
void ADC_Init(void);
void ADC_Config(void);
uint16_t ADC_Read(void);
float ConvertADCToTemperature(uint16_t adcValue);
void UART2_Init(void);
void UART2_Write(char ch);
void UART2_SendString(char* str);


int main( void )
{
	  UART2_Init();
    ADC_Init();
    ADC_Config();
    STM_EVAL_LEDInit(LED3);
	  STM_EVAL_LEDInit(LED4);
	
    xTaskCreate( vLED4, "LED 4", 128,NULL, tskIDLE_PRIORITY + 2, NULL );
    xTaskCreate( vLED3,"LED 3", 128, NULL,tskIDLE_PRIORITY + 2, NULL ); 
    xTaskCreate( vadc,"adc", 128, NULL,tskIDLE_PRIORITY + 1, NULL );  	
    vTaskStartScheduler();
}




 void vLED4( void *pvParameters )
{
   	pvParameters = pvParameters;
	while (1)
 	{
		if(temp > 25){
		  
			STM_EVAL_LEDOff(LED3);
		  STM_EVAL_LEDOn(LED4);	
		  vTaskDelay( 100 );
		
		}
		
	}
}

 void vLED3( void *pvParameters )
{
   	pvParameters = pvParameters;
	while (1)
 	{
		if(temp < 25){
		  
			STM_EVAL_LEDOff(LED4);
		  STM_EVAL_LEDOn(LED3);	
		  vTaskDelay( 100 );
		
		}
}

 void vadc( void *pvParameters )
{
   	pvParameters = pvParameters;
	while (1)
 	{
		    adcValue = ADC_Read();
        temp = ConvertADCToTemperature(adcValue);
		    UART2_SendString(temp);
 		
		
	}
}

void ADC_Init(void) {
    // Activer l'horloge pour l'ADC1 et GPIOA
    RCC->APB2ENR |= RCC_APB2ENR_ADC1EN; 
    RCC->AHB1ENR |= RCC_AHB1ENR_GPIOAEN; 

    // Configurer la broche PA0 en mode analogique
    GPIOA->MODER |= GPIO_MODER_MODER0; 
    GPIOA->PUPDR &= ~GPIO_PUPDR_PUPDR0; 
}

void ADC_Config(void) {
    
    ADC1->CR2 &= ~ADC_CR2_ADON;

    // Configuration de l'ADC
    ADC1->CR1 = 0; 
    ADC1->CR2 = 0; 

    ADC1->CR1 |= ADC_CR1_RES_0; 
    ADC1->CR2 |= ADC_CR2_CONT; 
    ADC1->SQR1 = 0; 
    ADC1->SQR3 = 0; 
    ADC1->SQR3 |= ADC_SQR3_SQ1_0; 

    
    ADC1->SMPR2 |= ADC_SMPR2_SMP0_0; 

    
    ADC1->CR2 |= ADC_CR2_ADON;
}

uint16_t ADC_Read(void) {
    
    ADC1->CR2 |= ADC_CR2_SWSTART;

    
    while (!(ADC1->SR & ADC_SR_EOC));

    
    return ADC1->DR;
}

float ConvertADCToTemperature(uint16_t adcValue) {
    // T=a*x + b   4095 ----> -20 °C   0 ----> 45 °C
    return -0.01587 * adcValue + 45.0;
}

void UART2_Init(void) {
    
    RCC->APB1ENR |= RCC_APB1ENR_USART2EN; 
    RCC->AHB1ENR |= RCC_AHB1ENR_GPIOAEN;  

    
    GPIOA->MODER &= ~(GPIO_MODER_MODER2 | GPIO_MODER_MODER3); 
    GPIOA->MODER |= (GPIO_MODER_MODER2_1 | GPIO_MODER_MODER3_1); 
    
    GPIOA->AFR[0] |= (7 << GPIO_AFRL_AFRL2_Pos) | (7 << GPIO_AFRL_AFRL3_Pos); 
    
    
    USART2->BRR = 0x0683; 
    USART2->CR1 = USART_CR1_TE | USART_CR1_RE | USART_CR1_UE; 

  }

void UART2_Write(char ch) {
    
    while (!(USART2->SR & USART_SR_TXE));
    USART2->DR = ch; 
}


void UART2_SendString(char* str) {
    while (*str) {
        UART2_Write(*str++);
    }
}











