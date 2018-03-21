var gulp = require('gulp');
var clean = require('gulp-clean');
var zip = require('gulp-zip');
var merge = require('merge-stream');
var ts = require("gulp-typescript");

gulp.task('clean', function () {
    var build = gulp.src('Site/out', { read: false })
        .pipe(clean());
    var dist = gulp.src('editor.zip', { read: false })
        .pipe(clean());

    return merge(build, dist);
});

gulp.task('compile', ['clean'], function () {
    var tsProject = ts.createProject("tsconfig.json")
    return tsProject
        .src()
        .pipe(tsProject())
        .js
        .pipe(gulp.dest(""));

    return merge(editor, diff);
});


gulp.task('build',['compile'], function () {
    var editorHtml = gulp.src('Site/editor.html')
        .pipe(gulp.dest('Site/out'));
    
    var diffHtml = gulp.src('Site/diff.html')
        .pipe(gulp.dest('Site/out'));
    
    var es6promise = gulp.src('node_modules/es6-promise/dist/es6-promise.auto.min.js')
        .pipe(gulp.dest('Site/out'));

    var monaco = gulp.src('node_modules/monaco-editor/min/vs/**')
        .pipe(gulp.dest('Site/out/vs'));

    return merge(editorHtml, diffHtml, es6promise, monaco);
});

gulp.task('zip', ['build'], function () {
    return gulp.src('Site/out/**')
        .pipe(zip('editor.zip'))
        .pipe(gulp.dest(''));
});

gulp.task('default', ['zip']);