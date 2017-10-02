def config_path = env.ConfigurationPath + "/HomeAutomation/ThermostatApi/"
def build = "staging"

node('windows') {
	stage('compile') {
		checkout scm //git 'https://github.com/hqv1/home-automation.git'
		
		dir("Thermostat") {
            bat 'dotnet clean ./Thermostat.sln'
            bat 'docker-compose -f docker-compose.ci.build.yml up'
        }
	}
	
	stage('unit test') {
		dir("Thermostat/tests/Api.Tests.Unit") {
            bat 'dotnet test --filter Category=Unit'
        }
	}
	
	stage('containers up') {
		dir("Thermostat") {
            bat 'docker-compose build'
            bat "docker-compose -f docker-compose.yml -f ${config_path}docker-compose.${build}.yml up -d --no-build"
		}
	}
	
	stage('integration tests') {
		dir("Thermostat") {            
            bat "newman run ${workspace}/Thermostat/Api/Postman/home-automation.postman_collection.json -e ${config_path}Staging.postman_environment.json"
        }
	}
	
	stage('push image') {
		dir("Thermostat") {                       
            bat "docker-compose push"
        }
	}
	
	stage('cleanup') {
		dir("Thermostat") {                                  
            bat "docker-compose down --rmi all"
        }
	}
}