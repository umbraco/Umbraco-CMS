angular.module("umbraco").controller("Umbraco.Starterkit.HelpController", function ($scope, helpService) {

    vm = this;
    vm.currentLesson = null;
    vm.currentStep = null;
    vm.currentStepIndex = 0;

    this.lessons = null;
    this.steps = null;
    
    this.loadLesson = function (lesson) {
        vm.currentLesson = lesson;
        helpService.getLessonSteps(lesson.path, "http://localhost:24292").then(function (steps) {
            vm.steps = steps;
            vm.currentStep = steps[0];
            vm.currentStepIndex = 0;
        });
    };
    
    this.exitLesson = function () {
        this.currentLesson = null;
        this.currentStep = null;
        this.steps = null;
    };

    this.loadStep = function (index) {
        this.currentStep = this.steps[index];
        this.currentStepIndex = index;
    };
    
    helpService.findLessons("Tutorials/Starter-kit/Lessons", "http://localhost:24292").then(function (lessons) {
        vm.lessons = lessons;
    });
});