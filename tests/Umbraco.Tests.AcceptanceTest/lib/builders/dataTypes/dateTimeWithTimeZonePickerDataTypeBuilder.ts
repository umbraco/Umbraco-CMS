import {DataTypeBuilder} from './dataTypeBuilder';

export class DateTimeWithTimeZonePickerDataTypeBuilder extends DataTypeBuilder {
  timeFormat: string;
  timeZoneMode: string;
  timeZones: string[];

  constructor() {
    super();
    this.editorAlias = 'Umbraco.DateTimeWithTimeZone';
    this.editorUiAlias = 'Umb.PropertyEditorUi.DateTimeWithTimeZonePicker';
    this.timeZones = [];
  }

  withTimeFormat(timeFormat: string) {
    this.timeFormat = timeFormat;
    return this;
  }

  withTimeZoneMode(timeZoneMode: string) {
    this.timeZoneMode = timeZoneMode;
    return this;
  }

  withTimeZone(timeZone: string) {
    this.timeZones.push(timeZone);
    return this;
  }

  getValues() {
    let values: any = [];
    values.push({
      alias: 'timeFormat',
      value: this.timeFormat || 'HH:mm'
    });

    values.push({
      alias: 'timeZones',
      value: {
        mode: this.timeZoneMode || 'all',
        timeZones: this.timeZones
      }
    });
    return values;
  }
}