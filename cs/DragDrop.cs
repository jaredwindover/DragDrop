(($) ->
  $.fn.getExpressions = ->
    #for all matching selectors
    this.map (ind,elem) ->
      #find the bbox-id class
      classes = elem.className.split /\s+/
      idClass = classes.filter((s) -> s.indexOf('bbox-') == 0)[0]
      #strip the id
      id = parseInt idClass[5..]
      #find the first MathJax-Color-xxx id element after
      dropDiv = $ elem
        .find '[id^="MathJax-Color"]:first'
      #set the expressionid in this element
      dropDiv.attr('data-expressionId',id)
      #return the dom element
      dropDiv.get()
) jQuery

ExpressionBuilder =
  #Go from an expression id back to the expression
  ExpressionMap: []
  #Make sure we give unique id's
  counter: 0
  #Build an Expression
  getExpression: (expression, link) ->
    count = @counter #increment after so that child expressions have unique ids
    @counter++
    #new expression from string
    console.log(link)
    if expression? then res  = new Expression expression, link, count
    #blank expression
    else res = new BlankExpression count
    #put expression into the map
    @ExpressionMap[count] = res
    return res
  #After mathjax has generated the html, prepare droppable expressions
   
  prepareDrops: ->
    #clear pointer events so only the right ones happen
    $ '.MathJax'
    .find '*'
    .css 'pointer-events', 'none'
    #fix up drops
    $ '.drop'
    .getExpressions()
    #add pointer events back
    .css 'pointer-events', 'visiblePainted'
    .addClass 'dropbox'
    .each (id,elem) ->
      #make each one droppable
      $(elem).droppable
        #just requires the mouse in the box
        tolerance : 'pointer'
        #function to call on drop
        drop: ((id,event,ui) ->
          #identify current expression by id
          curExp = ExpressionBuilder.ExpressionMap[id]
          #identify palette's expression
          newExp = ui.draggable.data 'expression'
          #copy palette expression
          newExp = ExpressionBuilder.getExpression(
            newExp.expression,
            newExp.link
          )
          #replace with new expression
          curExp.replace newExp
          prepareMain()
          #bind expressionid to id field in function
          ).bind(null, $(this).attr('data-expressionid'))
  prepareLinks: ->
    $ '.exp'
    .getExpressions()
    .css 'pointer-events', 'visiblePainted'
    .addClass 'explink'
    .each (id,elem) ->
      id = $(this).attr('data-expressionid')
      link = ExpressionBuilder.ExpressionMap[id].link
      $ elem
      .on 'click', ((link) ->
        console.log('Hi there')
        if link? then window.open link
      ).bind(null,link)
        
#virtual
class BaseExpression
  makePalette: () ->
    $ "<div class='palette'>
      $$#{@renderStatic()}$$
      </div>"
    .data 
      'expression': @


class BlankExpression extends BaseExpression
  constructor: (@id) ->
  #render expression to latex
  renderStatic: ->
    "\\Mark{#{@id}}{\\bbox[border:1px dashed black]{\\phantom{x}}}"
  #render expression to latex for interaction in mathjax
  renderDynamic: ->
    "\\Mark{#{@id} drop}{\\bbox[border:1px solid black]{\\phantom{x}}}"

class Expression extends BaseExpression
  constructor: (@expression,@link,@id) ->
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
    @setChild(@children[i - 1],i) for i in [@numChildren..1] by -1
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
    return "{\\Mark{#{@id} exp}{#{res}}}"

frac  =
  exp : '{\\frac{#1}{#2}}',
  link : 'https://www.google.com/?q=fraction'
add   =
  exp : '{#1+#2}'
  link : 'https://www.google.com/?q=addition'
sqrt  =
  exp : '{\\sqrt{#1}}'
  link : 'https://www.google.com/?q=squareroot'
exp   =
  exp : '{#1^#2}'
  link : 'https://www.google.com/?q=exponentiation'
sub   =
  exp : '{#1 - #2}'
  link : 'https://www.google.com/?q=subtraction'
mult  =
  exp : '{#1#2}'
  link : 'https://www.google.com/?q=multiplication'
sum   =
  exp : '{\\sum_{#1}^{#2}{#3}}'
  link : 'https://www.google.com/?q=sum'
paren =
  exp : '{\\left({#1}\\right)}'     
  link : 'https://www.google.com/?q=parentheses'
equal =
  exp : '{{#1}={#2}}'
  link : 'https://www.google.com/?q=equal'
x     =
  exp : '{x}'
  link : null
y     =
  exp : '{y}'
  link : null
a     =
  exp : '{a}'
  link : null
b     =
  exp : '{b}'
  link : null
neg   =
  exp : '{-{#1}}'
  link : 'https://www.google.com/?q=negative'
pm    =
  exp : '{{#1}\\pm{#2}}'
  link : 'https://www.google.com/?q=plusminus'

palettes = (
  ExpressionBuilder
    .getExpression e.exp, e.link
    .makePalette()  for e in [
      x, y, a, b,
      neg, sqrt,
      equal, paren, add, sub, pm, mult, frac, exp,
      sum]
)

main = '{#1}'
mainExpression = ExpressionBuilder
  .getExpression main


makeDraggable = ->
    $ '.palette'
      .draggable
        stack  : '.palette',
        revert : true

prepareMain = ->
  $ '.expression'
  .html "$$#{mainExpression.renderDynamic()}$$"
  MathJax.Hub.Queue(
    ['Typeset',MathJax.Hub, $('body').get()],
    ['prepareDrops',ExpressionBuilder],
    ['prepareLinks',ExpressionBuilder],
    makeDraggable)


$ document
  .ready ->
    $ '.palette-container'
      .append palettes
    prepareMain()
