import {DataTypeBuilder} from './dataTypeBuilder';
import {ImageCropperCropBuilder} from './imageCropperBuilder';

export class ImageCropperDataTypeBuilder extends DataTypeBuilder {
  imageCropperCropBuilder: ImageCropperCropBuilder[];

  constructor() {
    super();
    this.editorAlias = 'Umbraco.ImageCropper';
    this.editorUiAlias = 'Umb.PropertyEditorUi.ImageCropper';
    this.imageCropperCropBuilder = [];
  }

  addCrop() {
    const builder = new ImageCropperCropBuilder(this);
    this.imageCropperCropBuilder.push(builder);
    return builder;
  }

  getValues() {
    let values: any[] = [];
    if (this.imageCropperCropBuilder && this.imageCropperCropBuilder.length > 0) {
      values.push({
        alias: 'crops',
        value: this.imageCropperCropBuilder.map((builder) => {
          return builder.getValues();
        })
      });
    }
    return values;
  }
}
