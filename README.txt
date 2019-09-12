


-Interval posredovanja in prekrivanje obdobja z rednim delovnim časom
	Če vpišemo interval, ki prekriva redni delovni čas se ta na seznamu intervencij pojavi brez dela ki sega v redni delovni čas, 
	medtem ko na poročilu o opravljenem delu pa je izpisan celotni interval 
		primer1: 10:00 - 17:00 postane 15:00 - 17:00
		primer2: 6:00 - 17:00 postaneta dva intervala 6:00 - 7:00 in 15:00 - 17:00
		primer3: 10:00 - 12:00 se ne bo izpisal na seznamu intervencij bo zgolj zgenerirano poročilo

-Intervali posredovanja se prekrivajo
	Če vpišemo intervale ki se prekrivajo se prekrivajoče ure ne štejejo dvojno
		primer1: 3:00 - 8:00 in 4:00 - 9:00 izven delovnega časa pokrivata 4 ure ki se obračunajo kot intervencija
		