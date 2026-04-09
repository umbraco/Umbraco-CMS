import {TinyMCEDataTypeBuilder} from './tinyMCEDataTypeBuilder';

export class TinyMCEToolbarBuilder {
  parentBuilder: TinyMCEDataTypeBuilder;
  toolbarValues: {[key: string]: boolean} = {};

  constructor(parentBuilder: TinyMCEDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withUndo(undo: boolean) {
    this.toolbarValues['undo'] = undo;
    return this;
  }

  withRedo(redo: boolean) {
    this.toolbarValues['redo'] = redo;
    return this;
  }

  withCut(cut: boolean) {
    this.toolbarValues['cut'] = cut;
    return this;
  }

  withCopy(copy: boolean) {
    this.toolbarValues['copy'] = copy;
    return this;
  }

  withPaste(paste: boolean) {
    this.toolbarValues['paste'] = paste;
    return this;
  }

  withStyles(styles: boolean) {
    this.toolbarValues['styles'] = styles;
    return this;
  }

  withFontName(fontname: boolean) {
    this.toolbarValues['fontname'] = fontname;
    return this;
  }

  withFontsize(fontsize: boolean) {
    this.toolbarValues['fontsize'] = fontsize;
    return this;
  }
  
  withForeColor(forecolor: boolean) {
    this.toolbarValues['forecolor'] = forecolor;
    return this;
  }

  withBackcolor(backcolor: boolean) {
    this.toolbarValues['backcolor'] = backcolor;
    return this;
  }

  withBlockQuote(blockquote: boolean) {
    this.toolbarValues['blockquote'] = blockquote;
    return this;
  }

  withFormatQuote(formatquote: boolean) {
    this.toolbarValues['formatquote'] = formatquote;
    return this;
  }

  withRemoveFormat(removeformat: boolean) {
    this.toolbarValues['removeformat'] = removeformat;
    return this;
  }

  withBold(bold: boolean) {
    this.toolbarValues['bold'] = bold;
    return this;
  }

  withItalic(italic: boolean) {
    this.toolbarValues['italic'] = italic;
    return this;
  }

  withUnderline(underline: boolean) {
    this.toolbarValues['underline'] = underline;
    return this;
  }

  withStrikeThrough(strikethrough: boolean) {
    this.toolbarValues['strikethrough'] = strikethrough;
    return this;
  }

  withAlignLeft(alignleft: boolean) {
    this.toolbarValues['alignleft'] = alignleft;
    return this;
  }

  withAlignCenter(aligncenter: boolean) {
    this.toolbarValues['aligncenter'] = aligncenter;
    return this;
  }

  withAlignRight(alignright: boolean) {
    this.toolbarValues['alignright'] = alignright;
    return this;
  }

  withAlignJustify(alignjustify: boolean) {
    this.toolbarValues['alignjustify'] = alignjustify;
    return this;
  }

  withBulList(bullist: boolean) {
    this.toolbarValues['bullist'] = bullist;
    return this;
  }

  withNumList(numlist: boolean) {
    this.toolbarValues['numlist'] = numlist;
    return this;
  }

  withOutdent(outdent: boolean) {
    this.toolbarValues['outdent'] = outdent;
    return this;
  }

  withIndent(indent: boolean) {
    this.toolbarValues['indent'] = indent;
    return this;
  }

  withAnchor(anchor: boolean) {
    this.toolbarValues['anchor'] = anchor;
    return this;
  }

  withTable(table: boolean) {
    this.toolbarValues['table'] = table;
    return this;
  }

  withHr(hr: boolean) {
    this.toolbarValues['hr'] = hr;
    return this;
  }

  withSubscript(subscript: boolean) {
    this.toolbarValues['subscript'] = subscript;
    return this;
  }

  withSuperScript(superscript: boolean) {
    this.toolbarValues['superscript'] = superscript;
    return this;
  }

  withCharMap(charmap: boolean) {
    this.toolbarValues['charmap'] = charmap;
    return this;
  }

  withRTL(rtl: boolean) {
    this.toolbarValues['rtl'] = rtl;
    return this;
  }

  withLTR(ltr: boolean) {
    this.toolbarValues['ltr'] = ltr;
    return this;
  }

  withSourceCode(sourcecode: boolean) {
    this.toolbarValues['sourcecode'] = sourcecode;
    return this;
  }

  withUmbMediaPicker(umbmediapicker: boolean) {
    this.toolbarValues['umbmediapicker'] = umbmediapicker;
    return this;
  }

  withUmbEmbedDialog(umbembeddialog: boolean) {
    this.toolbarValues['umbembeddialog'] = umbembeddialog;
    return this;
  }

  withLink(link: boolean) {
    this.toolbarValues['link'] = link;
    return this;
  }

  withUnlink(unlink: boolean) {
    this.toolbarValues['unlink'] = unlink;
    return this;
  }

  withUmbBlockPicker(umbblockpicker: boolean) {
    this.toolbarValues['umbblockpicker'] = umbblockpicker;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    return Object.keys(this.toolbarValues).filter(key => this.toolbarValues[key]);
  }
}
