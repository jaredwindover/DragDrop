JADEDIR = templates/
COFFDIR = cs/
JAVADIR = public/js/
HTMLDIR = public/

COFF = $(wildcard $(COFFDIR)/*.cs)
JAVA = $(COFF:$(COFFDIR)/%.cs=$(JAVADIR)/%.js)
JADE = $(wildcard $(JADEDIR)/*.jade)
HTML = $(JADE:$(JADEDIR)/%.jade=$(HTMLDIR)/%.html)


all: jade coffee

jade: $(HTML)

coffee: $(JAVA)

public/js/%.js: $(COFFDIR)%.cs
	coffee -o $(JAVADIR) -c $< 

public/%.html: $(JADEDIR)%.jade
	jade $< -o public/

.PHONY: clean test clear start

clean:
	rm -rf *~

test:

clear: clean
	rm -f $(HTMLDIR)*.html
	rm -f $(JAVADIR)*

start:
	http-server public/
