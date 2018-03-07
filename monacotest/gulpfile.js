var gulp = require('gulp');
var clean = require('gulp-clean');
var zip = require('gulp-zip');
var merge = require('merge-stream');

gulp.task('clean', function () {
    var build = gulp.src('ts/out', { read: false })
        .pipe(clean());
    var dist = gulp.src('editor.zip', { read: false })
        .pipe(clean());

    return merge(build, dist);
});

gulp.task('build',['clean'], function () {
    var index = gulp.src('ts/index.html')
        .pipe(gulp.dest('ts/out'));

    var editor = gulp.src('ts/editor.js')
        .pipe(gulp.dest('ts/out'));

    var monaco = gulp.src('node_modules/monaco-editor/min/vs/**')
        .pipe(gulp.dest('ts/out/vs'));
    
    return merge(index, editor, monaco);
});

gulp.task('zip', ['build'], function () {
    return gulp.src('ts/out/**')
        .pipe(zip('editor.zip'))
        .pipe(gulp.dest(''));
});

gulp.task('default', ['zip']);