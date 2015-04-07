ExpressionBuilder =
  #Go from an expression id back to the expression
  ExpressionMap: []
  #Make sure we give unique id's
  counter: 0
  #Build an Expression
  getExpression: (expression) ->
    count = @counter
    @counter++
    if expression? then res  = new Expression expression, count
    else res = new BlankExpression count
    @ExpressionMap[count] = res
    return res
  #After mathjax has generated the html, prepare droppable expressions
  prepareDrops: ->
    $ '.MathJax'
    .find '*'
    .css 'pointer-events', 'none'
    $ '.drop'
    .each (ind,elem) ->
      classes = elem.className.split /\s+/
      idClass = classes.filter((s) -> s.indexOf('bbox-') == 0)[0]
      id = parseInt idClass[5..]
      dropDiv = $ '.' + idClass
        .find '[id^="MathJax-Color"]:first'
        .css 'pointer-events','visiblePainted'
        .addClass 'dropbox'
        .droppable
          tolerance : 'pointer'
          drop: ((id,event,ui) ->
            curExp = ExpressionBuilder.ExpressionMap[id]
            newExp = ui.draggable.data 'expression'
              .expression
            newExp = ExpressionBuilder.getExpression(newExp)
            curExp.replace newExp
            $ '.expression'
              .html "$$#{mainExpression.renderDynamic()}$$"
            MathJax.Hub.Queue(
              ['Typeset',MathJax.Hub, $('body').get()],
              ['prepareDrops',ExpressionBuilder],
              makeDragDrop)
          ).bind(null, id)
          
#virtual
class BaseExpression
  #virtual
  constructor: ->
  #virtual
  renderStatic: ->
  #virtual
  renderDynamic: ->
  makePalette: ->
    $(
      "<div class='palette'>
      $$#{@renderStatic()}$$
      </div>"
    ).data('expression', @)


class BlankExpression extends BaseExpression
  constructor: (@id) ->
  #render expression to latex
  renderStatic: ->
    "\\Mark{#{@id}}{\\bbox[border:1px dashed black]{\\phantom{x}}}"
  #render expression to latex for interaction in mathjax
  renderDynamic: ->
    "\\Mark{#{@id} drop}{\\bbox[border:1px solid black]{\\phantom{x}}}"

class Expression extends BaseExpression
  constructor: (@expression,@id) ->
    #count children in expression
    @numChildren = 1
    @numChildren++ while @hasn(@numChildren)
    @numChildren--

    #Initialize children as blank expressions
    @children = (
      ExpressionBuilder
      .getExpression() for i in [1..@numChildren] by 1
    )
    #Set their replace methods
    @setChild(child,@numChildren - i) for child, i in @children
    @children[0] = null

  #is there an nth child in the expression?
  hasn : (n) ->
    @expression.indexOf('#' + n) != -1
  #set child n to be childExp
  setChild: (childExp, n) ->
    if n > @numChildren + 1
      throw new RangeError(
        "Cannot assign to child #{n}.\n
        Only #{@numChildren} children."
      )
    else if n <= 0
      throw new RangeError(
        "Can only assign to chidlren >= 1."
      )
    else
      @children[n] = childExp
      childExp.replace = (newExp) =>
        @setChild(newExp,n)
  #render expression to latex
  renderStatic: ->
    res = @expression
    for i in [@numChildren..1] by -1
      res = res.replace('#' + i,@children[i].renderStatic())
    return "{\\Mark{#{@id}}{#{res}}}"
        
  #render expression to latex for interaction 
  renderDynamic: ->
    res = @expression
    for i in [@numChildren..1] by -1
      res = res.replace('#' + i,@children[i].renderDynamic())
    return "{\\Mark{#{@id}}{#{res}}}"


frac  = '{\\frac{#1}{#2}}'
add   = '{#1+#2}'
sqrt  = '{\\sqrt{#1}}'
exp   = '{#1^#2}'
sub   = '{#1 - #2}'
mult  = '{#1#2}'
sum   = '{\\sum_{#1}^{#2}{#3}}'
paren = '{\\left({#1}\\right)}'     
equal = '{{#1}={#2}}'
x     = '{x}'
y     = '{y}'
a     = '{a}'
b     = '{b}'
neg   = '{-{#1}}'
pm    = '{{#1}\\pm{#2}}'
blank = null

palettes = (
  ExpressionBuilder
    .getExpression e
    .makePalette()  for e in [
      x, y, a, b,
      neg, sqrt,
      equal, paren, add, sub, pm, mult, frac,
      sum]
)

main = '{#1}'
mainExpression = ExpressionBuilder
  .getExpression main


makeDragDrop = ->
    $ '.palette'
      .draggable
        stack  : '.palette',
        revert : true


$ document
  .ready ->
    makeDragDrop
    $ '.palette-container'
      .append palettes
    $ '.expression'
      .html "$$#{mainExpression.renderDynamic()}$$"
    MathJax.Hub.Queue(
      ['Typeset',MathJax.Hub,$('body').get()],
      ['prepareDrops',ExpressionBuilder],
      makeDragDrop
    )
