var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify");

var paths = {
    webroot: "./wwwroot/"
};

gulp.task("npmscripts", function () {
    var fs = require('fs');
    var json = JSON.parse(fs.readFileSync('./package.json'));
    var streams = [];

    for (var prop in json.dependencies) {
        console.log("Prepping Scripts for: " + prop);
        streams.push(gulp.src("node_modules/" + prop + "/**/*")
            .pipe(gulp.dest(paths.webroot + "nodemodules/" + prop)));
    }
});