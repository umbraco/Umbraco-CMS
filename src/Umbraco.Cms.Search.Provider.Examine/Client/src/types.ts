export interface ExamineField {
  name: string;
  type: string;
  values: Array<string>;
}

export interface ExamineIndexDocument {
  fields: Array<ExamineField>;
}

export interface ExamineDocument {
  documents: Array<ExamineIndexDocument>;
}

export interface ShowFieldsModalData {
  documentUnique: string;
  indexAlias: string;
  culture?: string;
}
