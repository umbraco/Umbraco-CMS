import {
  getContentItemByPath20,
  type ApiBlockItemModel,
  type IApiContentResponseModel,
  type TestPageContentResponseModel,
} from './api/umbraco-delivery';

(async () => {
  console.log('** Page - Default **');
  const content = (await getContentItemByPath20('/', {expand: 'properties[$all]'})).data;
  renderPage(content);
})();

function renderPage(content: IApiContentResponseModel) {
  console.log('  Name: ', content.name);
  console.log('  Path: ', content.route?.path);

  if (content.contentType === 'testPage') {
    renderTestPage(content);
  }
}

function renderTestPage(content: TestPageContentResponseModel) {
  const {properties} = content;

  console.log('\n  **Common**');
  print('textString', properties?.textString);
  print('textArea', properties?.textArea);
  print('datePickerWithTime', properties?.datePickerWithTime);
  print('datePicker', properties?.datePicker);
  print('toggle', properties?.toggle);
  print('numeric', properties?.numeric);
  print('decimal', properties?.decimal);
  print('slider', properties?.slider);
  print('tags', properties?.tags);
  print('email', properties?.email);
  print('dateOnly', properties?.dateOnly);
  print('timeOnly', properties?.timeOnly);
  print('dateTimeUnspecified', properties?.dateTimeUnspecified);
  print('dateTimeWithTimeZone', properties?.dateTimeWithTimeZone);

  console.log('\n  **Pickers**');
  print('colorPicker', properties?.colorPicker);
  print('contentPicker', '<tested below>');
  print('eyeDropperColorPicker', properties?.eyeDropperColorPicker);
  print('urlPicker', properties?.urlPicker);
  print('multinodeTreepicker', '<tested below>');
  print('userPicker', properties?.userPicker);

  console.log('\n  **Rich content**');
  print('richText', properties?.richText);
  print('blockGrid', '<tested below>');
  print('markdown', properties?.markdown);

  console.log('\n  **Lists**');
  print('blockList', '<tested below>');
  print('checkboxList', properties?.checkboxList);
  print('dropdown', properties?.dropdown);
  print('radiobox', properties?.radiobox);
  print('repeatableTextstrings', properties?.repeatableTextstrings);

  console.log('\n  **Media**');
  print('uploadFile', properties?.uploadFile);
  print('imageCropper', properties?.imageCropper);
  print('mediaPicker', properties?.mediaPicker);

  console.log('\n  **Content Picker**');
  print('name', properties?.contentPicker?.name);
  print('route>path', properties?.contentPicker?.route?.path);

  console.log('\n  **Multinode Treepicker**');
  print('name', properties?.multinodeTreepicker?.[0]?.name);
  print('route>path', properties?.multinodeTreepicker?.[0]?.route?.path);

  console.log('\n  **Block List**');
  properties?.blockList?.items?.forEach((block, i) => {
    console.log(`    Block[${i}]:`);
    renderBlock(block);
  });

  console.log('\n  **Block Grid**');
  properties?.blockGrid?.items?.forEach((block, i) => {
    console.log(`    Block[${i}]:`);
    renderBlock(block);
  });

  console.log('\n  **From compositions**');
  print('  sharedToggle', properties?.sharedToggle);
  print('  sharedString', properties?.sharedString);
  print('  sharedRadiobox', properties?.sharedRadiobox);
  print('  sharedRichText', properties?.sharedRichText);
}

function renderBlock(block: ApiBlockItemModel) {
  console.log('      Type: ', block.content?.contentType);
  switch (block.content?.contentType) {
    case 'testBlock': {
      console.log('      String: ', block.content.properties?.string);
      console.log('      Multinode Treepicker: ', block.content.properties?.multinodeTreepicker?.[0]?.id);
      console.log('      Shared string: ', block.content.properties?.sharedString);
      const nestedBlock = block.content.properties?.blocks?.items?.[0];
      if (nestedBlock) {
        console.log('      **Nested block**');
        renderBlock(nestedBlock);
      }

      break;
    }

    case 'testBlock2': {
      console.log('      Shared string (testBlock2): ', block.content.properties?.sharedString);
      if (block.settings?.contentType === 'blockSettings') {
        console.log('      Anchor id (settings): ', block.settings.properties?.anchorId);
      }

      break;
    }

    default:
      console.error('      Unknown block type');
      break;
  }
}

function print(propertyName: string, value: unknown) {
  console.log(`    ${propertyName} (${typeof value}): ${JSON.stringify(value)}`);
}
