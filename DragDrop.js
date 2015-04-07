var ExpressionBuilder = {
    ExpressionMap : [],
    counter : 0,
    getExpression : function(expression){
	var count = this.counter;
	this.counter ++ ;
	if (expression === undefined) {
	    var res = new BlankExpression(count);
	} else {
	    var res = new Expression(expression,count);
	}
	this.ExpressionMap[count] = res; 
	return res;
    },
    prepareDrops: function(){
	$('.MathJax')
	    .find("*")
	    .css('pointer-events','none');
	$('.drop').each(function(ind,elem){
	    classes = elem.className.split(/\s+/);
	    idClass = classes.
		filter(
		    function(s){
			return s.indexOf('bbox-') === 0;
		    })[0];
	    id = parseInt(idClass.slice(5));
	    dropDiv = $('.'+idClass).find("[id^='MathJax-Color']:first");
	    dropDiv.css('pointer-events','visiblePainted');
	    dropDiv.addClass('dropbox');
	    dropDiv.droppable({
		tolerance:'pointer',
		drop:function(id,event,ui){
		    console.log(id);
		    curExp = ExpressionBuilder.ExpressionMap[id];
		    console.log(curExp);
		    newExp = ui.draggable.data('expression').get().expression;
		    newExp = ExpressionBuilder.getExpression(newExp);
		    curExp.replace(newExp);
		    $('.expression').html('$$' + mainExpression.renderDynamic() + '$$');
		    MathJax.Hub.Queue(["Typeset",
				       MathJax.Hub,
				       $('body').get()],
				      ['prepareDrops',ExpressionBuilder],
				      makeDragDrop);
		}.bind(null,id)
	    });
	    console.log(dropDiv);
	    console.log(id);
	});
    }
}

function BlankExpression(id){
    this.id = id;
    this.renderStatic = function(){
	return '\\Mark{' + this.id + '}' + '{\\bbox[border:1px dashed black]{\\phantom{x}}}';
    }
    this.renderDynamic = function(){
	return '\\Mark{' + this.id + ' drop}' + '{\\bbox[border:1px solid black]{\\phantom{x}}}';
    }
}

function Expression(expression, id){
    this.id = id;
    this.expression = expression;
    this.hasn = function(n){
	ind = this.expression.indexOf('#' + n);
	return -1 !== ind;
    };
    var num = 1;
    while (this.hasn(num)){
	num++
    }
    this.numChildren = num - 1;
    this.children = new Array(this.numChildren);
    this.setChild = function(exp,n){
	if (n > this.numChildren + 1){
	    throw new RangeError(
		"Cannot assign to child " + n + 
		    ". Only " + this.numChildren + " children.");
	} else if (n <= 0){
	    throw new RangeErro("Can only assign to children >= 1");
	} else {
	    this.children[n] = exp;
	    exp.replace = function(newExp){
		this.children[n] = newExp;
	    }.bind(this);
	}
    };
    for (var i = 1; i < this.numChildren + 1; i++){
	this.setChild(ExpressionBuilder.getExpression(),i);
    }       
    this.renderStatic = function(){
	res = this.expression;
	for (var i = this.numChildren; i >= 1; i--){
	    res=res.
		replace('#' + i,
			this.children[i].renderStatic());
	}
	return '{\\Mark{' + this.id + '}' + '{' + res + '}}';
    }
    this.renderDynamic = function(){
	res = this.expression;
	for (var i = this.numChildren; i >= 1; i--){
	    res=res.
		replace('#' + i,
			this.children[i].renderDynamic());
	}
	return '{\\Mark{' + this.id + '}' + '{' + res + '}}';
    }
}

var fracExpression = ExpressionBuilder.
    getExpression('{\\frac{#1}{#2}}');
var addExpression = ExpressionBuilder.
    getExpression('{#1+#2}');
var sqrtExpression = ExpressionBuilder.
    getExpression('{\\sqrt{#1}}');
var expExpression = ExpressionBuilder.
    getExpression('{#1^#2}');
var subExpression = ExpressionBuilder.
    getExpression('{#1 - #2}');
var multExpression = ExpressionBuilder.
    getExpression('{#1#2}');
var sumExpression = ExpressionBuilder.
    getExpression('{\\sum_{#1}^{#2}{#3}}');
var mainExpression = ExpressionBuilder.
    getExpression('{#1}');
var parenExpression = ExpressionBuilder.getExpression('{(#1)}');
var equalExpression = ExpressionBuilder.getExpression('{{#1}={#2}}');
var xExpression = ExpressionBuilder.getExpression('{x}');
var yExpression = ExpressionBuilder.getExpression('{y}');
var aExpression = ExpressionBuilder.getExpression('{a}');
var bExpression = ExpressionBuilder.getExpression('{b}');
var negExpression = ExpressionBuilder.getExpression('{-{#1}}');
var pmExpression = ExpressionBuilder.getExpression('{{#1}\\pm{#2}}');
var blankExpression = ExpressionBuilder.getExpression();

mainExpression.setChild(blankExpression, 1);

function makePalette(exp){
    pal = $("<div class='palette'>$$" +
	    exp.renderStatic() +
	    '$$</div>');
    pal.data('expression',new Reference(exp));
    return pal;
}

function Reference(obj){
    this.get = function(){
	return obj;
    }
}

function makeDragDrop(){
    $('.palette').draggable(
	{
	    stack:'.palette',
	    revert:true
	}
    );
}

$(document).ready(function(){
    makeDragDrop();
    $('.palette-container').append(
	makePalette(xExpression),
	makePalette(yExpression),
	makePalette(aExpression),
	makePalette(bExpression),
	makePalette(negExpression),
	makePalette(equalExpression),
	makePalette(addExpression),
	makePalette(subExpression),
	makePalette(pmExpression),
	makePalette(multExpression),
	makePalette(fracExpression),
	makePalette(expExpression),
	makePalette(sqrtExpression),
	makePalette(sumExpression),
	makePalette(parenExpression)
    );
    $('.expression').html('$$' + mainExpression.renderDynamic() + '$$');
    MathJax.Hub.Queue(["Typeset",
		       MathJax.Hub,
		       $('body').get()],
		      ['prepareDrops',ExpressionBuilder],
		      makeDragDrop);
})
