/* flatpickr v4.5.2, @license MIT */
(function (global, factory) {
    typeof exports === 'object' && typeof module !== 'undefined' ? factory(exports) :
    typeof define === 'function' && define.amd ? define(['exports'], factory) :
    (factory((global.index = {})));
}(this, (function (exports) { 'use strict';

    var fp = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Arabic = {
      weekdays: {
        shorthand: ["أحد", "اثنين", "ثلاثاء", "أربعاء", "خميس", "جمعة", "سبت"],
        longhand: ["الأحد", "الاثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت"]
      },
      months: {
        shorthand: ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"],
        longhand: ["يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"]
      }
    };
    fp.l10ns.ar = Arabic;
    fp.l10ns;

    var fp$1 = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Austria = {
      weekdays: {
        shorthand: ["So", "Mo", "Di", "Mi", "Do", "Fr", "Sa"],
        longhand: ["Sonntag", "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag"]
      },
      months: {
        shorthand: ["Jän", "Feb", "Mär", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dez"],
        longhand: ["Jänner", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember"]
      },
      firstDayOfWeek: 1,
      weekAbbreviation: "KW",
      rangeSeparator: " bis ",
      scrollTitle: "Zum Ändern scrollen",
      toggleTitle: "Zum Umschalten klicken"
    };
    fp$1.l10ns.at = Austria;
    fp$1.l10ns;

    var fp$2 = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Belarusian = {
      weekdays: {
        shorthand: ["Нд", "Пн", "Аў", "Ср", "Чц", "Пт", "Сб"],
        longhand: ["Нядзеля", "Панядзелак", "Аўторак", "Серада", "Чацвер", "Пятніца", "Субота"]
      },
      months: {
        shorthand: ["Сту", "Лют", "Сак", "Кра", "Тра", "Чэр", "Ліп", "Жні", "Вер", "Кас", "Ліс", "Сне"],
        longhand: ["Студзень", "Люты", "Сакавік", "Красавік", "Травень", "Чэрвень", "Ліпень", "Жнівень", "Верасень", "Кастрычнік", "Лістапад", "Снежань"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return "";
      },
      rangeSeparator: " — ",
      weekAbbreviation: "Тыд.",
      scrollTitle: "Пракруціце для павелічэння",
      toggleTitle: "Націсніце для пераключэння",
      amPM: ["ДП", "ПП"],
      yearAriaLabel: "Год"
    };
    fp$2.l10ns.be = Belarusian;
    fp$2.l10ns;

    var fp$3 = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Bulgarian = {
      weekdays: {
        shorthand: ["Нд", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"],
        longhand: ["Неделя", "Понеделник", "Вторник", "Сряда", "Четвъртък", "Петък", "Събота"]
      },
      months: {
        shorthand: ["Яну", "Фев", "Март", "Апр", "Май", "Юни", "Юли", "Авг", "Сеп", "Окт", "Ное", "Дек"],
        longhand: ["Януари", "Февруари", "Март", "Април", "Май", "Юни", "Юли", "Август", "Септември", "Октомври", "Ноември", "Декември"]
      }
    };
    fp$3.l10ns.bg = Bulgarian;
    fp$3.l10ns;

    var fp$4 = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Bangla = {
      weekdays: {
        shorthand: ["রবি", "সোম", "মঙ্গল", "বুধ", "বৃহস্পতি", "শুক্র", "শনি"],
        longhand: ["রবিবার", "সোমবার", "মঙ্গলবার", "বুধবার", "বৃহস্পতিবার", "শুক্রবার", "শনিবার"]
      },
      months: {
        shorthand: ["জানু", "ফেব্রু", "মার্চ", "এপ্রিল", "মে", "জুন", "জুলাই", "আগ", "সেপ্টে", "অক্টো", "নভে", "ডিসে"],
        longhand: ["জানুয়ারী", "ফেব্রুয়ারী", "মার্চ", "এপ্রিল", "মে", "জুন", "জুলাই", "আগস্ট", "সেপ্টেম্বর", "অক্টোবর", "নভেম্বর", "ডিসেম্বর"]
      }
    };
    fp$4.l10ns.bn = Bangla;
    fp$4.l10ns;

    var fp$5 = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Catalan = {
      weekdays: {
        shorthand: ["Dg", "Dl", "Dt", "Dc", "Dj", "Dv", "Ds"],
        longhand: ["Diumenge", "Dilluns", "Dimarts", "Dimecres", "Dijous", "Divendres", "Dissabte"]
      },
      months: {
        shorthand: ["Gen", "Febr", "Març", "Abr", "Maig", "Juny", "Jul", "Ag", "Set", "Oct", "Nov", "Des"],
        longhand: ["Gener", "Febrer", "Març", "Abril", "Maig", "Juny", "Juliol", "Agost", "Setembre", "Octubre", "Novembre", "Desembre"]
      },
      ordinal: function ordinal(nth) {
        var s = nth % 100;
        if (s > 3 && s < 21) return "è";

        switch (s % 10) {
          case 1:
            return "r";

          case 2:
            return "n";

          case 3:
            return "r";

          case 4:
            return "t";

          default:
            return "è";
        }
      },
      firstDayOfWeek: 1
    };
    fp$5.l10ns.cat = Catalan;
    fp$5.l10ns;

    var fp$6 = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Czech = {
      weekdays: {
        shorthand: ["Ne", "Po", "Út", "St", "Čt", "Pá", "So"],
        longhand: ["Neděle", "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota"]
      },
      months: {
        shorthand: ["Led", "Ún", "Bře", "Dub", "Kvě", "Čer", "Čvc", "Srp", "Zář", "Říj", "Lis", "Pro"],
        longhand: ["Leden", "Únor", "Březen", "Duben", "Květen", "Červen", "Červenec", "Srpen", "Září", "Říjen", "Listopad", "Prosinec"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return ".";
      },
      rangeSeparator: " do ",
      weekAbbreviation: "Týd.",
      scrollTitle: "Rolujte pro změnu",
      toggleTitle: "Přepnout dopoledne/odpoledne",
      amPM: ["dop.", "odp."],
      yearAriaLabel: "Rok"
    };
    fp$6.l10ns.cs = Czech;
    fp$6.l10ns;

    var fp$7 = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Welsh = {
      weekdays: {
        shorthand: ["Sul", "Llun", "Maw", "Mer", "Iau", "Gwe", "Sad"],
        longhand: ["Dydd Sul", "Dydd Llun", "Dydd Mawrth", "Dydd Mercher", "Dydd Iau", "Dydd Gwener", "Dydd Sadwrn"]
      },
      months: {
        shorthand: ["Ion", "Chwef", "Maw", "Ebr", "Mai", "Meh", "Gorff", "Awst", "Medi", "Hyd", "Tach", "Rhag"],
        longhand: ["Ionawr", "Chwefror", "Mawrth", "Ebrill", "Mai", "Mehefin", "Gorffennaf", "Awst", "Medi", "Hydref", "Tachwedd", "Rhagfyr"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal(nth) {
        if (nth === 1) return "af";
        if (nth === 2) return "ail";
        if (nth === 3 || nth === 4) return "ydd";
        if (nth === 5 || nth === 6) return "ed";
        if (nth >= 7 && nth <= 10 || nth == 12 || nth == 15 || nth == 18 || nth == 20) return "fed";
        if (nth == 11 || nth == 13 || nth == 14 || nth == 16 || nth == 17 || nth == 19) return "eg";
        if (nth >= 21 && nth <= 39) return "ain";
        return "";
      }
    };
    fp$7.l10ns.cy = Welsh;
    fp$7.l10ns;

    var fp$8 = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Danish = {
      weekdays: {
        shorthand: ["søn", "man", "tir", "ons", "tors", "fre", "lør"],
        longhand: ["søndag", "mandag", "tirsdag", "onsdag", "torsdag", "fredag", "lørdag"]
      },
      months: {
        shorthand: ["jan", "feb", "mar", "apr", "maj", "jun", "jul", "aug", "sep", "okt", "nov", "dec"],
        longhand: ["januar", "februar", "marts", "april", "maj", "juni", "juli", "august", "september", "oktober", "november", "december"]
      },
      ordinal: function ordinal() {
        return ".";
      },
      firstDayOfWeek: 1,
      rangeSeparator: " til ",
      weekAbbreviation: "uge"
    };
    fp$8.l10ns.da = Danish;
    fp$8.l10ns;

    var fp$9 = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var German = {
      weekdays: {
        shorthand: ["So", "Mo", "Di", "Mi", "Do", "Fr", "Sa"],
        longhand: ["Sonntag", "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag"]
      },
      months: {
        shorthand: ["Jan", "Feb", "Mär", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dez"],
        longhand: ["Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember"]
      },
      firstDayOfWeek: 1,
      weekAbbreviation: "KW",
      rangeSeparator: " bis ",
      scrollTitle: "Zum Ändern scrollen",
      toggleTitle: "Zum Umschalten klicken"
    };
    fp$9.l10ns.de = German;
    fp$9.l10ns;

    var english = {
      weekdays: {
        shorthand: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"],
        longhand: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]
      },
      months: {
        shorthand: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
        longhand: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]
      },
      daysInMonth: [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31],
      firstDayOfWeek: 0,
      ordinal: function ordinal(nth) {
        var s = nth % 100;
        if (s > 3 && s < 21) return "th";

        switch (s % 10) {
          case 1:
            return "st";

          case 2:
            return "nd";

          case 3:
            return "rd";

          default:
            return "th";
        }
      },
      rangeSeparator: " to ",
      weekAbbreviation: "Wk",
      scrollTitle: "Scroll to increment",
      toggleTitle: "Click to toggle",
      amPM: ["AM", "PM"],
      yearAriaLabel: "Year"
    };

    var fp$a = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Esperanto = {
      firstDayOfWeek: 1,
      rangeSeparator: " ĝis ",
      weekAbbreviation: "Sem",
      scrollTitle: "Rulumu por pligrandigi la valoron",
      toggleTitle: "Klaku por ŝalti",
      weekdays: {
        shorthand: ["Dim", "Lun", "Mar", "Mer", "Ĵaŭ", "Ven", "Sab"],
        longhand: ["dimanĉo", "lundo", "mardo", "merkredo", "ĵaŭdo", "vendredo", "sabato"]
      },
      months: {
        shorthand: ["Jan", "Feb", "Mar", "Apr", "Maj", "Jun", "Jul", "Aŭg", "Sep", "Okt", "Nov", "Dec"],
        longhand: ["januaro", "februaro", "marto", "aprilo", "majo", "junio", "julio", "aŭgusto", "septembro", "oktobro", "novembro", "decembro"]
      },
      ordinal: function ordinal() {
        return "-a";
      }
    };
    fp$a.l10ns.eo = Esperanto;
    fp$a.l10ns;

    var fp$b = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Spanish = {
      weekdays: {
        shorthand: ["Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"],
        longhand: ["Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"]
      },
      months: {
        shorthand: ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"],
        longhand: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"]
      },
      ordinal: function ordinal() {
        return "º";
      },
      firstDayOfWeek: 1,
      rangeSeparator: " a "
    };
    fp$b.l10ns.es = Spanish;
    fp$b.l10ns;

    var fp$c = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Estonian = {
      weekdays: {
        shorthand: ["P", "E", "T", "K", "N", "R", "L"],
        longhand: ["Pühapäev", "Esmaspäev", "Teisipäev", "Kolmapäev", "Neljapäev", "Reede", "Laupäev"]
      },
      months: {
        shorthand: ["Jaan", "Veebr", "Märts", "Apr", "Mai", "Juuni", "Juuli", "Aug", "Sept", "Okt", "Nov", "Dets"],
        longhand: ["Jaanuar", "Veebruar", "Märts", "Aprill", "Mai", "Juuni", "Juuli", "August", "September", "Oktoober", "November", "Detsember"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return ".";
      },
      weekAbbreviation: "Näd",
      rangeSeparator: " kuni ",
      scrollTitle: "Keri, et suurendada",
      toggleTitle: "Klõpsa, et vahetada"
    };
    fp$c.l10ns.et = Estonian;
    fp$c.l10ns;

    var fp$d = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Persian = {
      weekdays: {
        shorthand: ["یک", "دو", "سه", "چهار", "پنج", "جمعه", "شنبه"],
        longhand: ["یک‌شنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنچ‌شنبه", "جمعه", "شنبه"]
      },
      months: {
        shorthand: ["ژانویه", "فوریه", "مارس", "آوریل", "مه", "ژوئن", "ژوئیه", "اوت", "سپتامبر", "اکتبر", "نوامبر", "دسامبر"],
        longhand: ["ژانویه", "فوریه", "مارس", "آوریل", "مه", "ژوئن", "ژوئیه", "اوت", "سپتامبر", "اکتبر", "نوامبر", "دسامبر"]
      },
      firstDayOfWeek: 6,
      ordinal: function ordinal() {
        return "";
      }
    };
    fp$d.l10ns.fa = Persian;
    fp$d.l10ns;

    var fp$e = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Finnish = {
      firstDayOfWeek: 1,
      weekdays: {
        shorthand: ["Su", "Ma", "Ti", "Ke", "To", "Pe", "La"],
        longhand: ["Sunnuntai", "Maanantai", "Tiistai", "Keskiviikko", "Torstai", "Perjantai", "Lauantai"]
      },
      months: {
        shorthand: ["Tammi", "Helmi", "Maalis", "Huhti", "Touko", "Kesä", "Heinä", "Elo", "Syys", "Loka", "Marras", "Joulu"],
        longhand: ["Tammikuu", "Helmikuu", "Maaliskuu", "Huhtikuu", "Toukokuu", "Kesäkuu", "Heinäkuu", "Elokuu", "Syyskuu", "Lokakuu", "Marraskuu", "Joulukuu"]
      },
      ordinal: function ordinal() {
        return ".";
      }
    };
    fp$e.l10ns.fi = Finnish;
    fp$e.l10ns;

    var fp$f = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var French = {
      firstDayOfWeek: 1,
      weekdays: {
        shorthand: ["dim", "lun", "mar", "mer", "jeu", "ven", "sam"],
        longhand: ["dimanche", "lundi", "mardi", "mercredi", "jeudi", "vendredi", "samedi"]
      },
      months: {
        shorthand: ["janv", "févr", "mars", "avr", "mai", "juin", "juil", "août", "sept", "oct", "nov", "déc"],
        longhand: ["janvier", "février", "mars", "avril", "mai", "juin", "juillet", "août", "septembre", "octobre", "novembre", "décembre"]
      },
      ordinal: function ordinal(nth) {
        if (nth > 1) return "";
        return "er";
      },
      rangeSeparator: " au ",
      weekAbbreviation: "Sem",
      scrollTitle: "Défiler pour augmenter la valeur",
      toggleTitle: "Cliquer pour basculer"
    };
    fp$f.l10ns.fr = French;
    fp$f.l10ns;

    var fp$g = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Greek = {
      weekdays: {
        shorthand: ["Κυ", "Δε", "Τρ", "Τε", "Πέ", "Πα", "Σά"],
        longhand: ["Κυριακή", "Δευτέρα", "Τρίτη", "Τετάρτη", "Πέμπτη", "Παρασκευή", "Σάββατο"]
      },
      months: {
        shorthand: ["Ιαν", "Φεβ", "Μάρ", "Απρ", "Μάι", "Ιού", "Ιού", "Αύγ", "Σεπ", "Οκτ", "Νοέ", "Δεκ"],
        longhand: ["Ιανουάριος", "Φεβρουάριος", "Μάρτιος", "Απρίλιος", "Μάιος", "Ιούνιος", "Ιούλιος", "Αύγουστος", "Σεπτέμβριος", "Οκτώβριος", "Νοέμβριος", "Δεκέμβριος"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return "";
      },
      weekAbbreviation: "Εβδ",
      rangeSeparator: " έως ",
      scrollTitle: "Μετακυλήστε για προσαύξηση",
      toggleTitle: "Κάντε κλικ για αλλαγή",
      amPM: ["ΠΜ", "ΜΜ"]
    };
    fp$g.l10ns.gr = Greek;
    fp$g.l10ns;

    var fp$h = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Hebrew = {
      weekdays: {
        shorthand: ["א", "ב", "ג", "ד", "ה", "ו", "ז"],
        longhand: ["ראשון", "שני", "שלישי", "רביעי", "חמישי", "שישי", "שבת"]
      },
      months: {
        shorthand: ["ינו׳", "פבר׳", "מרץ", "אפר׳", "מאי", "יוני", "יולי", "אוג׳", "ספט׳", "אוק׳", "נוב׳", "דצמ׳"],
        longhand: ["ינואר", "פברואר", "מרץ", "אפריל", "מאי", "יוני", "יולי", "אוגוסט", "ספטמבר", "אוקטובר", "נובמבר", "דצמבר"]
      }
    };
    fp$h.l10ns.he = Hebrew;
    fp$h.l10ns;

    var fp$i = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Hindi = {
      weekdays: {
        shorthand: ["रवि", "सोम", "मंगल", "बुध", "गुरु", "शुक्र", "शनि"],
        longhand: ["रविवार", "सोमवार", "मंगलवार", "बुधवार", "गुरुवार", "शुक्रवार", "शनिवार"]
      },
      months: {
        shorthand: ["जन", "फर", "मार्च", "अप्रेल", "मई", "जून", "जूलाई", "अग", "सित", "अक्ट", "नव", "दि"],
        longhand: ["जनवरी ", "फरवरी", "मार्च", "अप्रेल", "मई", "जून", "जूलाई", "अगस्त ", "सितम्बर", "अक्टूबर", "नवम्बर", "दिसम्बर"]
      }
    };
    fp$i.l10ns.hi = Hindi;
    fp$i.l10ns;

    var fp$j = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Croatian = {
      firstDayOfWeek: 1,
      weekdays: {
        shorthand: ["Ned", "Pon", "Uto", "Sri", "Čet", "Pet", "Sub"],
        longhand: ["Nedjelja", "Ponedjeljak", "Utorak", "Srijeda", "Četvrtak", "Petak", "Subota"]
      },
      months: {
        shorthand: ["Sij", "Velj", "Ožu", "Tra", "Svi", "Lip", "Srp", "Kol", "Ruj", "Lis", "Stu", "Pro"],
        longhand: ["Siječanj", "Veljača", "Ožujak", "Travanj", "Svibanj", "Lipanj", "Srpanj", "Kolovoz", "Rujan", "Listopad", "Studeni", "Prosinac"]
      }
    };
    fp$j.l10ns.hr = Croatian;
    fp$j.l10ns;

    var fp$k = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Hungarian = {
      firstDayOfWeek: 1,
      weekdays: {
        shorthand: ["V", "H", "K", "Sz", "Cs", "P", "Szo"],
        longhand: ["Vasárnap", "Hétfő", "Kedd", "Szerda", "Csütörtök", "Péntek", "Szombat"]
      },
      months: {
        shorthand: ["Jan", "Feb", "Már", "Ápr", "Máj", "Jún", "Júl", "Aug", "Szep", "Okt", "Nov", "Dec"],
        longhand: ["Január", "Február", "Március", "Április", "Május", "Június", "Július", "Augusztus", "Szeptember", "Október", "November", "December"]
      },
      ordinal: function ordinal() {
        return ".";
      },
      weekAbbreviation: "Hét",
      scrollTitle: "Görgessen",
      toggleTitle: "Kattintson a váltáshoz"
    };
    fp$k.l10ns.hu = Hungarian;
    fp$k.l10ns;

    var fp$l = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Indonesian = {
      weekdays: {
        shorthand: ["Min", "Sen", "Sel", "Rab", "Kam", "Jum", "Sab"],
        longhand: ["Minggu", "Senin", "Selasa", "Rabu", "Kamis", "Jumat", "Sabtu"]
      },
      months: {
        shorthand: ["Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des"],
        longhand: ["Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "November", "Desember"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return "";
      }
    };
    fp$l.l10ns.id = Indonesian;
    fp$l.l10ns;

    var fp$m = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Italian = {
      weekdays: {
        shorthand: ["Dom", "Lun", "Mar", "Mer", "Gio", "Ven", "Sab"],
        longhand: ["Domenica", "Lunedì", "Martedì", "Mercoledì", "Giovedì", "Venerdì", "Sabato"]
      },
      months: {
        shorthand: ["Gen", "Feb", "Mar", "Apr", "Mag", "Giu", "Lug", "Ago", "Set", "Ott", "Nov", "Dic"],
        longhand: ["Gennaio", "Febbraio", "Marzo", "Aprile", "Maggio", "Giugno", "Luglio", "Agosto", "Settembre", "Ottobre", "Novembre", "Dicembre"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return "°";
      },
      rangeSeparator: " al ",
      weekAbbreviation: "Se",
      scrollTitle: "Scrolla per aumentare",
      toggleTitle: "Clicca per cambiare"
    };
    fp$m.l10ns.it = Italian;
    fp$m.l10ns;

    var fp$n = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Japanese = {
      weekdays: {
        shorthand: ["日", "月", "火", "水", "木", "金", "土"],
        longhand: ["日曜日", "月曜日", "火曜日", "水曜日", "木曜日", "金曜日", "土曜日"]
      },
      months: {
        shorthand: ["1月", "2月", "3月", "4月", "5月", "6月", "7月", "8月", "9月", "10月", "11月", "12月"],
        longhand: ["1月", "2月", "3月", "4月", "5月", "6月", "7月", "8月", "9月", "10月", "11月", "12月"]
      }
    };
    fp$n.l10ns.ja = Japanese;
    fp$n.l10ns;

    var fp$o = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Korean = {
      weekdays: {
        shorthand: ["일", "월", "화", "수", "목", "금", "토"],
        longhand: ["일요일", "월요일", "화요일", "수요일", "목요일", "금요일", "토요일"]
      },
      months: {
        shorthand: ["1월", "2월", "3월", "4월", "5월", "6월", "7월", "8월", "9월", "10월", "11월", "12월"],
        longhand: ["1월", "2월", "3월", "4월", "5월", "6월", "7월", "8월", "9월", "10월", "11월", "12월"]
      },
      ordinal: function ordinal() {
        return "일";
      }
    };
    fp$o.l10ns.ko = Korean;
    fp$o.l10ns;

    var fp$p = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Khmer = {
      weekdays: {
        shorthand: ["អាទិត្យ", "ចន្ទ", "អង្គារ", "ពុធ", "ព្រហស.", "សុក្រ", "សៅរ៍"],
        longhand: ["អាទិត្យ", "ចន្ទ", "អង្គារ", "ពុធ", "ព្រហស្បតិ៍", "សុក្រ", "សៅរ៍"]
      },
      months: {
        shorthand: ["មករា", "កុម្ភះ", "មីនា", "មេសា", "ឧសភា", "មិថុនា", "កក្កដា", "សីហា", "កញ្ញា", "តុលា", "វិច្ឆិកា", "ធ្នូ"],
        longhand: ["មករា", "កុម្ភះ", "មីនា", "មេសា", "ឧសភា", "មិថុនា", "កក្កដា", "សីហា", "កញ្ញា", "តុលា", "វិច្ឆិកា", "ធ្នូ"]
      },
      ordinal: function ordinal() {
        return "";
      },
      firstDayOfWeek: 1,
      rangeSeparator: " ដល់ ",
      weekAbbreviation: "សប្តាហ៍",
      scrollTitle: "រំកិលដើម្បីបង្កើន",
      toggleTitle: "ចុចដើម្បីផ្លាស់ប្ដូរ",
      yearAriaLabel: "ឆ្នាំ"
    };
    fp$p.l10ns.km = Khmer;
    fp$p.l10ns;

    var fp$q = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Kazakh = {
      weekdays: {
        shorthand: ["Жс", "Дс", "Сc", "Ср", "Бс", "Жм", "Сб"],
        longhand: ["Жексенбi", "Дүйсенбi", "Сейсенбi", "Сәрсенбi", "Бейсенбi", "Жұма", "Сенбi"]
      },
      months: {
        shorthand: ["Қаң", "Ақп", "Нау", "Сәу", "Мам", "Мау", "Шiл", "Там", "Қыр", "Қаз", "Қар", "Жел"],
        longhand: ["Қаңтар", "Ақпан", "Наурыз", "Сәуiр", "Мамыр", "Маусым", "Шiлде", "Тамыз", "Қыркүйек", "Қазан", "Қараша", "Желтоқсан"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return "";
      },
      rangeSeparator: " — ",
      weekAbbreviation: "Апта",
      scrollTitle: "Үлкейту үшін айналдырыңыз",
      toggleTitle: "Ауыстыру үшін басыңыз",
      amPM: ["ТД", "ТК"],
      yearAriaLabel: "Жыл"
    };
    fp$q.l10ns.kz = Kazakh;
    fp$q.l10ns;

    var fp$r = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Lithuanian = {
      weekdays: {
        shorthand: ["S", "Pr", "A", "T", "K", "Pn", "Š"],
        longhand: ["Sekmadienis", "Pirmadienis", "Antradienis", "Trečiadienis", "Ketvirtadienis", "Penktadienis", "Šeštadienis"]
      },
      months: {
        shorthand: ["Sau", "Vas", "Kov", "Bal", "Geg", "Bir", "Lie", "Rgp", "Rgs", "Spl", "Lap", "Grd"],
        longhand: ["Sausis", "Vasaris", "Kovas", "Balandis", "Gegužė", "Birželis", "Liepa", "Rugpjūtis", "Rugsėjis", "Spalis", "Lapkritis", "Gruodis"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return "-a";
      },
      weekAbbreviation: "Sav",
      scrollTitle: "Keisti laiką pelės rateliu",
      toggleTitle: "Perjungti laiko formatą"
    };
    fp$r.l10ns.lt = Lithuanian;
    fp$r.l10ns;

    var fp$s = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Latvian = {
      firstDayOfWeek: 1,
      weekdays: {
        shorthand: ["Sv", "P", "Ot", "Tr", "Ce", "Pk", "Se"],
        longhand: ["Svētdiena", "Pirmdiena", "Otrdiena", "Trešdiena", "Ceturtdiena", "Piektdiena", "Sestdiena"]
      },
      months: {
        shorthand: ["Jan", "Feb", "Mar", "Mai", "Apr", "Jūn", "Jūl", "Aug", "Sep", "Okt", "Nov", "Dec"],
        longhand: ["Janvāris", "Februāris", "Marts", "Aprīlis", "Maijs", "Jūnijs", "Jūlijs", "Augusts", "Septembris", "Oktobris", "Novembris", "Decembris"]
      },
      rangeSeparator: " līdz "
    };
    fp$s.l10ns.lv = Latvian;
    fp$s.l10ns;

    var fp$t = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Macedonian = {
      weekdays: {
        shorthand: ["Не", "По", "Вт", "Ср", "Че", "Пе", "Са"],
        longhand: ["Недела", "Понеделник", "Вторник", "Среда", "Четврток", "Петок", "Сабота"]
      },
      months: {
        shorthand: ["Јан", "Фев", "Мар", "Апр", "Мај", "Јун", "Јул", "Авг", "Сеп", "Окт", "Ное", "Дек"],
        longhand: ["Јануари", "Февруари", "Март", "Април", "Мај", "Јуни", "Јули", "Август", "Септември", "Октомври", "Ноември", "Декември"]
      },
      firstDayOfWeek: 1,
      weekAbbreviation: "Нед.",
      rangeSeparator: " до "
    };
    fp$t.l10ns.mk = Macedonian;
    fp$t.l10ns;

    var fp$u = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Mongolian = {
      firstDayOfWeek: 1,
      weekdays: {
        shorthand: ["Да", "Мя", "Лх", "Пү", "Ба", "Бя", "Ня"],
        longhand: ["Даваа", "Мягмар", "Лхагва", "Пүрэв", "Баасан", "Бямба", "Ням"]
      },
      months: {
        shorthand: ["1-р сар", "2-р сар", "3-р сар", "4-р сар", "5-р сар", "6-р сар", "7-р сар", "8-р сар", "9-р сар", "10-р сар", "11-р сар", "12-р сар"],
        longhand: ["Нэгдүгээр сар", "Хоёрдугаар сар", "Гуравдугаар сар", "Дөрөвдүгээр сар", "Тавдугаар сар", "Зургаадугаар сар", "Долдугаар сар", "Наймдугаар сар", "Есдүгээр сар", "Аравдугаар сар", "Арваннэгдүгээр сар", "Арванхоёрдугаар сар"]
      },
      rangeSeparator: "-с "
    };
    fp$u.l10ns.mn = Mongolian;
    fp$u.l10ns;

    var fp$v = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Malaysian = {
      weekdays: {
        shorthand: ["Min", "Isn", "Sel", "Rab", "Kha", "Jum", "Sab"],
        longhand: ["Minggu", "Isnin", "Selasa", "Rabu", "Khamis", "Jumaat", "Sabtu"]
      },
      months: {
        shorthand: ["Jan", "Feb", "Mac", "Apr", "Mei", "Jun", "Jul", "Ogo", "Sep", "Okt", "Nov", "Dis"],
        longhand: ["Januari", "Februari", "Mac", "April", "Mei", "Jun", "Julai", "Ogos", "September", "Oktober", "November", "Disember"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return "";
      }
    };
    fp$v.l10ns;

    var fp$w = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Burmese = {
      weekdays: {
        shorthand: ["နွေ", "လာ", "ဂါ", "ဟူး", "ကြာ", "သော", "နေ"],
        longhand: ["တနင်္ဂနွေ", "တနင်္လာ", "အင်္ဂါ", "ဗုဒ္ဓဟူး", "ကြာသပတေး", "သောကြာ", "စနေ"]
      },
      months: {
        shorthand: ["ဇန်", "ဖေ", "မတ်", "ပြီ", "မေ", "ဇွန်", "လိုင်", "သြ", "စက်", "အောက်", "နို", "ဒီ"],
        longhand: ["ဇန်နဝါရီ", "ဖေဖော်ဝါရီ", "မတ်", "ဧပြီ", "မေ", "ဇွန်", "ဇူလိုင်", "သြဂုတ်", "စက်တင်ဘာ", "အောက်တိုဘာ", "နိုဝင်ဘာ", "ဒီဇင်ဘာ"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return "";
      }
    };
    fp$w.l10ns.my = Burmese;
    fp$w.l10ns;

    var fp$x = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Dutch = {
      weekdays: {
        shorthand: ["zo", "ma", "di", "wo", "do", "vr", "za"],
        longhand: ["zondag", "maandag", "dinsdag", "woensdag", "donderdag", "vrijdag", "zaterdag"]
      },
      months: {
        shorthand: ["jan", "feb", "mrt", "apr", "mei", "jun", "jul", "aug", "sept", "okt", "nov", "dec"],
        longhand: ["januari", "februari", "maart", "april", "mei", "juni", "juli", "augustus", "september", "oktober", "november", "december"]
      },
      firstDayOfWeek: 1,
      weekAbbreviation: "wk",
      rangeSeparator: " tot ",
      scrollTitle: "Scroll voor volgende / vorige",
      toggleTitle: "Klik om te wisselen",
      ordinal: function ordinal(nth) {
        if (nth === 1 || nth === 8 || nth >= 20) return "ste";
        return "de";
      }
    };
    fp$x.l10ns.nl = Dutch;
    fp$x.l10ns;

    var fp$y = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Norwegian = {
      weekdays: {
        shorthand: ["Søn", "Man", "Tir", "Ons", "Tor", "Fre", "Lør"],
        longhand: ["Søndag", "Mandag", "Tirsdag", "Onsdag", "Torsdag", "Fredag", "Lørdag"]
      },
      months: {
        shorthand: ["Jan", "Feb", "Mar", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Des"],
        longhand: ["Januar", "Februar", "Mars", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Desember"]
      },
      firstDayOfWeek: 1,
      rangeSeparator: " til ",
      weekAbbreviation: "Uke",
      scrollTitle: "Scroll for å endre",
      toggleTitle: "Klikk for å veksle",
      ordinal: function ordinal() {
        return ".";
      }
    };
    fp$y.l10ns.no = Norwegian;
    fp$y.l10ns;

    var fp$z = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Punjabi = {
      weekdays: {
        shorthand: ["ਐਤ", "ਸੋਮ", "ਮੰਗਲ", "ਬੁੱਧ", "ਵੀਰ", "ਸ਼ੁੱਕਰ", "ਸ਼ਨਿੱਚਰ"],
        longhand: ["ਐਤਵਾਰ", "ਸੋਮਵਾਰ", "ਮੰਗਲਵਾਰ", "ਬੁੱਧਵਾਰ", "ਵੀਰਵਾਰ", "ਸ਼ੁੱਕਰਵਾਰ", "ਸ਼ਨਿੱਚਰਵਾਰ"]
      },
      months: {
        shorthand: ["ਜਨ", "ਫ਼ਰ", "ਮਾਰ", "ਅਪ੍ਰੈ", "ਮਈ", "ਜੂਨ", "ਜੁਲਾ", "ਅਗ", "ਸਤੰ", "ਅਕ", "ਨਵੰ", "ਦਸੰ"],
        longhand: ["ਜਨਵਰੀ", "ਫ਼ਰਵਰੀ", "ਮਾਰਚ", "ਅਪ੍ਰੈਲ", "ਮਈ", "ਜੂਨ", "ਜੁਲਾਈ", "ਅਗਸਤ", "ਸਤੰਬਰ", "ਅਕਤੂਬਰ", "ਨਵੰਬਰ", "ਦਸੰਬਰ"]
      }
    };
    fp$z.l10ns.pa = Punjabi;
    fp$z.l10ns;

    var fp$A = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Polish = {
      weekdays: {
        shorthand: ["Nd", "Pn", "Wt", "Śr", "Cz", "Pt", "So"],
        longhand: ["Niedziela", "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek", "Sobota"]
      },
      months: {
        shorthand: ["Sty", "Lut", "Mar", "Kwi", "Maj", "Cze", "Lip", "Sie", "Wrz", "Paź", "Lis", "Gru"],
        longhand: ["Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec", "Lipiec", "Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień"]
      },
      rangeSeparator: " do ",
      weekAbbreviation: "tydz.",
      scrollTitle: "Przwiń aby zwiększyć",
      toggleTitle: "Kliknij aby przełączyć",
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return ".";
      }
    };
    fp$A.l10ns.pl = Polish;
    fp$A.l10ns;

    var fp$B = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Portuguese = {
      weekdays: {
        shorthand: ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"],
        longhand: ["Domingo", "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira", "Sábado"]
      },
      months: {
        shorthand: ["Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez"],
        longhand: ["Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"]
      },
      rangeSeparator: " até "
    };
    fp$B.l10ns.pt = Portuguese;
    fp$B.l10ns;

    var fp$C = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Romanian = {
      weekdays: {
        shorthand: ["Dum", "Lun", "Mar", "Mie", "Joi", "Vin", "Sam"],
        longhand: ["Duminică", "Luni", "Marți", "Miercuri", "Joi", "Vineri", "Sâmbătă"]
      },
      months: {
        shorthand: ["Ian", "Feb", "Mar", "Apr", "Mai", "Iun", "Iul", "Aug", "Sep", "Oct", "Noi", "Dec"],
        longhand: ["Ianuarie", "Februarie", "Martie", "Aprilie", "Mai", "Iunie", "Iulie", "August", "Septembrie", "Octombrie", "Noiembrie", "Decembrie"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return "";
      }
    };
    fp$C.l10ns.ro = Romanian;
    fp$C.l10ns;

    var fp$D = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Russian = {
      weekdays: {
        shorthand: ["Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"],
        longhand: ["Воскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота"]
      },
      months: {
        shorthand: ["Янв", "Фев", "Март", "Апр", "Май", "Июнь", "Июль", "Авг", "Сен", "Окт", "Ноя", "Дек"],
        longhand: ["Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return "";
      },
      rangeSeparator: " — ",
      weekAbbreviation: "Нед.",
      scrollTitle: "Прокрутите для увеличения",
      toggleTitle: "Нажмите для переключения",
      amPM: ["ДП", "ПП"],
      yearAriaLabel: "Год"
    };
    fp$D.l10ns.ru = Russian;
    fp$D.l10ns;

    var fp$E = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Sinhala = {
      weekdays: {
        shorthand: ["ඉ", "ස", "අ", "බ", "බ්‍ර", "සි", "සෙ"],
        longhand: ["ඉරිදා", "සඳුදා", "අඟහරුවාදා", "බදාදා", "බ්‍රහස්පතින්දා", "සිකුරාදා", "සෙනසුරාදා"]
      },
      months: {
        shorthand: ["ජන", "පෙබ", "මාර්", "අප්‍රේ", "මැයි", "ජුනි", "ජූලි", "අගෝ", "සැප්", "ඔක්", "නොවැ", "දෙසැ"],
        longhand: ["ජනවාරි", "පෙබරවාරි", "මාර්තු", "අප්‍රේල්", "මැයි", "ජුනි", "ජූලි", "අගෝස්තු", "සැප්තැම්බර්", "ඔක්තෝබර්", "නොවැම්බර්", "දෙසැම්බර්"]
      }
    };
    fp$E.l10ns.si = Sinhala;
    fp$E.l10ns;

    var fp$F = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Slovak = {
      weekdays: {
        shorthand: ["Ned", "Pon", "Ut", "Str", "Štv", "Pia", "Sob"],
        longhand: ["Nedeľa", "Pondelok", "Utorok", "Streda", "Štvrtok", "Piatok", "Sobota"]
      },
      months: {
        shorthand: ["Jan", "Feb", "Mar", "Apr", "Máj", "Jún", "Júl", "Aug", "Sep", "Okt", "Nov", "Dec"],
        longhand: ["Január", "Február", "Marec", "Apríl", "Máj", "Jún", "Júl", "August", "September", "Október", "November", "December"]
      },
      firstDayOfWeek: 1,
      rangeSeparator: " do ",
      ordinal: function ordinal() {
        return ".";
      }
    };
    fp$F.l10ns.sk = Slovak;
    fp$F.l10ns;

    var fp$G = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Slovenian = {
      weekdays: {
        shorthand: ["Ned", "Pon", "Tor", "Sre", "Čet", "Pet", "Sob"],
        longhand: ["Nedelja", "Ponedeljek", "Torek", "Sreda", "Četrtek", "Petek", "Sobota"]
      },
      months: {
        shorthand: ["Jan", "Feb", "Mar", "Apr", "Maj", "Jun", "Jul", "Avg", "Sep", "Okt", "Nov", "Dec"],
        longhand: ["Januar", "Februar", "Marec", "April", "Maj", "Junij", "Julij", "Avgust", "September", "Oktober", "November", "December"]
      },
      firstDayOfWeek: 1,
      rangeSeparator: " do ",
      ordinal: function ordinal() {
        return ".";
      }
    };
    fp$G.l10ns.sl = Slovenian;
    fp$G.l10ns;

    var fp$H = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Albanian = {
      weekdays: {
        shorthand: ["Di", "Hë", "Ma", "Më", "En", "Pr", "Sh"],
        longhand: ["E Diel", "E Hënë", "E Martë", "E Mërkurë", "E Enjte", "E Premte", "E Shtunë"]
      },
      months: {
        shorthand: ["Jan", "Shk", "Mar", "Pri", "Maj", "Qer", "Kor", "Gus", "Sht", "Tet", "Nën", "Dhj"],
        longhand: ["Janar", "Shkurt", "Mars", "Prill", "Maj", "Qershor", "Korrik", "Gusht", "Shtator", "Tetor", "Nëntor", "Dhjetor"]
      }
    };
    fp$H.l10ns.sq = Albanian;
    fp$H.l10ns;

    var fp$I = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Serbian = {
      weekdays: {
        shorthand: ["Ned", "Pon", "Uto", "Sre", "Čet", "Pet", "Sub"],
        longhand: ["Nedelja", "Ponedeljak", "Utorak", "Sreda", "Četvrtak", "Petak", "Subota"]
      },
      months: {
        shorthand: ["Jan", "Feb", "Mar", "Apr", "Maj", "Jun", "Jul", "Avg", "Sep", "Okt", "Nov", "Dec"],
        longhand: ["Januar", "Februar", "Mart", "April", "Maj", "Jun", "Jul", "Avgust", "Septembar", "Oktobar", "Novembar", "Decembar"]
      },
      firstDayOfWeek: 1,
      weekAbbreviation: "Ned.",
      rangeSeparator: " do "
    };
    fp$I.l10ns.sr = Serbian;
    fp$I.l10ns;

    var fp$J = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Swedish = {
      firstDayOfWeek: 1,
      weekAbbreviation: "v",
      weekdays: {
        shorthand: ["Sön", "Mån", "Tis", "Ons", "Tor", "Fre", "Lör"],
        longhand: ["Söndag", "Måndag", "Tisdag", "Onsdag", "Torsdag", "Fredag", "Lördag"]
      },
      months: {
        shorthand: ["Jan", "Feb", "Mar", "Apr", "Maj", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dec"],
        longhand: ["Januari", "Februari", "Mars", "April", "Maj", "Juni", "Juli", "Augusti", "September", "Oktober", "November", "December"]
      },
      ordinal: function ordinal() {
        return ".";
      }
    };
    fp$J.l10ns.sv = Swedish;
    fp$J.l10ns;

    var fp$K = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Thai = {
      weekdays: {
        shorthand: ["อา", "จ", "อ", "พ", "พฤ", "ศ", "ส"],
        longhand: ["อาทิตย์", "จันทร์", "อังคาร", "พุธ", "พฤหัสบดี", "ศุกร์", "เสาร์"]
      },
      months: {
        shorthand: ["ม.ค.", "ก.พ.", "มี.ค.", "เม.ย.", "พ.ค.", "มิ.ย.", "ก.ค.", "ส.ค.", "ก.ย.", "ต.ค.", "พ.ย.", "ธ.ค."],
        longhand: ["มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม"]
      },
      firstDayOfWeek: 1,
      rangeSeparator: " ถึง ",
      scrollTitle: "เลื่อนเพื่อเพิ่มหรือลด",
      toggleTitle: "คลิกเพื่อเปลี่ยน",
      ordinal: function ordinal() {
        return "";
      }
    };
    fp$K.l10ns.th = Thai;
    fp$K.l10ns;

    var fp$L = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Turkish = {
      weekdays: {
        shorthand: ["Paz", "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt"],
        longhand: ["Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi"]
      },
      months: {
        shorthand: ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"],
        longhand: ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"]
      },
      firstDayOfWeek: 1,
      ordinal: function ordinal() {
        return ".";
      },
      rangeSeparator: " - ",
      weekAbbreviation: "Hf",
      scrollTitle: "Artırmak için kaydırın",
      toggleTitle: "Aç/Kapa",
      amPM: ["ÖÖ", "ÖS"]
    };
    fp$L.l10ns.tr = Turkish;
    fp$L.l10ns;

    var fp$M = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Ukrainian = {
      firstDayOfWeek: 1,
      weekdays: {
        shorthand: ["Нд", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"],
        longhand: ["Неділя", "Понеділок", "Вівторок", "Середа", "Четвер", "П'ятниця", "Субота"]
      },
      months: {
        shorthand: ["Січ", "Лют", "Бер", "Кві", "Тра", "Чер", "Лип", "Сер", "Вер", "Жов", "Лис", "Гру"],
        longhand: ["Січень", "Лютий", "Березень", "Квітень", "Травень", "Червень", "Липень", "Серпень", "Вересень", "Жовтень", "Листопад", "Грудень"]
      }
    };
    fp$M.l10ns.uk = Ukrainian;
    fp$M.l10ns;

    var fp$N = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Vietnamese = {
      weekdays: {
        shorthand: ["CN", "T2", "T3", "T4", "T5", "T6", "T7"],
        longhand: ["Chủ nhật", "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy"]
      },
      months: {
        shorthand: ["Th1", "Th2", "Th3", "Th4", "Th5", "Th6", "Th7", "Th8", "Th9", "Th10", "Th11", "Th12"],
        longhand: ["Tháng một", "Tháng hai", "Tháng ba", "Tháng tư", "Tháng năm", "Tháng sáu", "Tháng bảy", "Tháng tám", "Tháng chín", "Tháng mười", "Tháng 11", "Tháng 12"]
      },
      firstDayOfWeek: 1
    };
    fp$N.l10ns.vn = Vietnamese;
    fp$N.l10ns;

    var fp$O = typeof window !== "undefined" && window.flatpickr !== undefined ? window.flatpickr : {
      l10ns: {}
    };
    var Mandarin = {
      weekdays: {
        shorthand: ["周日", "周一", "周二", "周三", "周四", "周五", "周六"],
        longhand: ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"]
      },
      months: {
        shorthand: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"],
        longhand: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"]
      },
      rangeSeparator: " 至 ",
      weekAbbreviation: "周",
      scrollTitle: "滚动切换",
      toggleTitle: "点击切换 12/24 小时时制"
    };
    fp$O.l10ns.zh = Mandarin;
    fp$O.l10ns;

    var l10n = {
      ar: Arabic,
      at: Austria,
      be: Belarusian,
      bg: Bulgarian,
      bn: Bangla,
      cat: Catalan,
      cs: Czech,
      cy: Welsh,
      da: Danish,
      de: German,
      default: Object.assign({}, english),
      en: english,
      eo: Esperanto,
      es: Spanish,
      et: Estonian,
      fa: Persian,
      fi: Finnish,
      fr: French,
      gr: Greek,
      he: Hebrew,
      hi: Hindi,
      hr: Croatian,
      hu: Hungarian,
      id: Indonesian,
      it: Italian,
      ja: Japanese,
      ko: Korean,
      km: Khmer,
      kz: Kazakh,
      lt: Lithuanian,
      lv: Latvian,
      mk: Macedonian,
      mn: Mongolian,
      ms: Malaysian,
      my: Burmese,
      nl: Dutch,
      no: Norwegian,
      pa: Punjabi,
      pl: Polish,
      pt: Portuguese,
      ro: Romanian,
      ru: Russian,
      si: Sinhala,
      sk: Slovak,
      sl: Slovenian,
      sq: Albanian,
      sr: Serbian,
      sv: Swedish,
      th: Thai,
      tr: Turkish,
      uk: Ukrainian,
      vn: Vietnamese,
      zh: Mandarin
    };

    exports.default = l10n;

    Object.defineProperty(exports, '__esModule', { value: true });

})));
