import {ApiHelpers} from "./ApiHelpers";
import {
  CheckboxListDataTypeBuilder,
  DatePickerDataTypeBuilder,
  BlockListDataTypeBuilder,
  DropdownDataTypeBuilder,
  ContentPickerDataTypeBuilder,
  BlockGridDataTypeBuilder,
  ImageCropperDataTypeBuilder,
  MediaPickerDataTypeBuilder,
  RadioboxDataTypeBuilder,
  TextAreaDataTypeBuilder,
  TextStringDataTypeBuilder,
  TrueFalseDataTypeBuilder,
  EmailAddressDataTypeBuilder,
  CodeEditorDataTypeBuilder,
  MarkdownEditorDataTypeBuilder,
  DecimalDataTypeBuilder,
  MultipleTextStringDataTypeBuilder,
  SliderDataTypeBuilder,
  ListViewDataTypeBuilder,
  TiptapDataTypeBuilder,
  TinyMCEDataTypeBuilder,
  ApprovedColorDataTypeBuilder,
  UploadFieldDataTypeBuilder,
  LabelDataTypeBuilder,
  MultiUrlPickerDataTypeBuilder,
  NumericDataTypeBuilder,
  TagsDataTypeBuilder,
  MultiNodeTreePickerDataTypeBuilder,
  DateTimeWithTimeZonePickerDataTypeBuilder, EntityDataPickerDataTypeBuilder
} from "../builders";
import {AliasHelper} from "./AliasHelper";

export class DataTypeApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/data-type/' + id);
    return await response.json();
  }

  async create(name: string, editorAlias: string, editorUiAlias: string, values: { alias: string; value: string; }[], parentId?: string, id?: string) {
    const dataType = {
      "name": name,
      "editorAlias": editorAlias,
      "editorUiAlias": editorUiAlias,
      "values": values,
      "id": id,
      "parent": parentId ? {"id": parentId} : null
    };
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/data-type', dataType);
    // Returns the id of the created dataType
    return response.headers().location.split("v1/data-type/").pop();
  }

  async update(id: string, dataType) {
    const updateDataType = {
      "name": dataType.name,
      "editorAlias": dataType.editorAlias,
      "editorUiAlias": dataType.editorUiAlias,
      "values": dataType.values,
    };
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/data-type/' + id, updateDataType);
  }

  async delete(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/data-type/' + id);
  }

  async getAllAtRoot() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/data-type/root?skip=0&take=10000&foldersOnly=false');
  }

  async doesExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/data-type/' + id);
    return response.status() === 200;
  }

  async getItems(ids: string[]) {
    let idArray = 'id=' + ids[0];
    let i: number;

    for (i = 1; i < ids.length; ++i) {
      idArray += '&id=' + ids[i];
    }

    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/data-type/item?' + idArray);
    return await response.json();
  }

  async getByName(name: string) {
    const rootDataTypes = await this.getAllAtRoot();
    const jsonDataTypes = await rootDataTypes.json();

    for (const dataType of jsonDataTypes.items) {
      if (dataType.name === name) {
        return this.get(dataType.id);
      } else if (dataType.isContainer || dataType.hasChildren) {
        const result = await this.recurseChildren(name, dataType.id, false);
        if (result) {
          return result;
        }
      }
    }
    return false;
  }

  async ensureNameNotExists(name: string) {
    const rootDataTypes = await this.getAllAtRoot();
    const jsonDataTypes = await rootDataTypes.json();

    for (const dataType of jsonDataTypes.items) {
      if (dataType.name === name) {
        if (dataType.isFolder) {
          return await this.recurseDeleteChildren(dataType);
        }
        return await this.delete(dataType.id);
      } else if (dataType.hasChildren) {
        await this.recurseChildren(name, dataType.id, true);

      }
    }
    return null;
  }

  async doesNameExist(name: string) {
    return await this.getByName(name);
  }

  async moveToFolder(dataTypeId: string, folderId: string) {
    const folderIdBody = {
      "target": {id: folderId}
    };
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/data-type/' + dataTypeId + '/move', folderIdBody);
  }

  async copyToFolder(dataTypeId: string, folderId: string) {
    const folderIdBody = {
      "target": {id: folderId}
    };
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/data-type/' + dataTypeId + '/copy', folderIdBody);
    // Returns the id of the copied dataType
    return response.headers().location.split("v1/data-type/").pop();
  }

  // FOLDER
  async getFolder(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/data-type/folder/' + id);
    return await response.json();
  }

  async createFolder(name: string, parentId?: string, id?: string) {
    const folderData = {
      "name": name,
      "id": id,
      "parent": parentId ? {"id": parentId} : null
    };

    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/data-type/folder', folderData);
    // Returns the id of the created dataTypeFolder
    return response.headers().location.split("v1/data-type/folder/").pop();
  }

  async renameFolder(id: string, name: string) {
    const folderData = {
      "name": name,
    };
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/data-type/folder/' + id, folderData);
  }

  async deleteFolder(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/data-type/folder/' + id);
  }

  async doesFolderExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/data-type/folder/' + id);
    return response.status() === 200;
  }

  async getChildren(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/data-type/children?parentId=' + id + '&skip=0&take=100&foldersOnly=false');
    const items = await response.json();
    return items.items;
  }

  private async recurseDeleteChildren(dataFolder) {
    if (!dataFolder.hasChildren) {
      return await this.deleteFolder(dataFolder.id);
    }
    const items = await this.getChildren(dataFolder.id);

    for (const child of items) {
      if (child.hasChildren) {
        await this.recurseDeleteChildren(child);
      } else if (child.isFolder) {
        await this.deleteFolder(child.id);
      } else {
        await this.delete(child.id);
      }
    }
    return await this.deleteFolder(dataFolder.id);
  }

  private async recurseChildren(name: string, id: string, toDelete: boolean) {
    const items = await this.getChildren(id);

    for (const child of items) {
      if (child.name === name) {
        if (!toDelete) {
          if (child.isFolder) {
            return await this.getFolder(child.id);
          }
          return await this.get(child.id);
        }
        if (child.isFolder) {
          return await this.recurseDeleteChildren(child);
        }
        return await this.delete(child.id);
      } else if (child.hasChildren) {
        return await this.recurseChildren(name, child.id, toDelete);
      }
    }
    return false;
  }

  async save(dataType) {
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/data-type', dataType)
    return response.headers().location.split("v1/data-type/").pop();
  }

  async createDefaultDateTimeDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new DatePickerDataTypeBuilder()
      .withName(name)
      .build();
    return await this.save(dataType);
  }

  async createCheckboxListDataType(name: string, options: string[] = []) {
    await this.ensureNameNotExists(name);

    const dataType = new CheckboxListDataTypeBuilder()
      .withName(name)
      .withItems(options)
      .build();
    return await this.save(dataType);
  }

  async createContentPickerDataTypeWithStartNode(name: string, startNodeId: string) {
    await this.ensureNameNotExists(name);

    const dataType = new ContentPickerDataTypeBuilder()
      .withName(name)
      .withStartNodeId(startNodeId)
      .build();
    return await this.save(dataType);
  }

  async createContentPickerDataTypeWithShowOpenButton(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new ContentPickerDataTypeBuilder()
      .withName(name)
      .withShowOpenButton(true)
      .build();
    return await this.save(dataType);
  }

  async createContentPickerDataTypeWithIgnoreUserStartNodes(name: string, startNodeId: string) {
    await this.ensureNameNotExists(name);

    const dataType = new ContentPickerDataTypeBuilder()
      .withName(name)
      .withStartNodeId(startNodeId)
      .withIgnoreUserStartNodes(true)
      .build();
    return await this.save(dataType);
  }
  
  async createDateTimeDataTypeWithDateFormat(name: string, dateFormat: string) {
    await this.ensureNameNotExists(name);

    const dataType = new DatePickerDataTypeBuilder()
      .withName(name)
      .withFormat(dateFormat)
      .build();
    return await this.save(dataType);
  }

  async createDropdownDataType(name: string, isMultiple: boolean, options: string[]) {
    await this.ensureNameNotExists(name);

    const dataType = new DropdownDataTypeBuilder()
      .withName(name)
      .withMultiple(isMultiple)
      .withItems(options)
      .build();
    return await this.save(dataType);
  }

  // BlockListEditor
  async createEmptyBlockListDataType(name: string) {
    await this.ensureNameNotExists(name);

    const blockList = new BlockListDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(blockList);
  }

  async createBlockListDataTypeWithTwoBlocks(name: string, firstContentElementTypeId: string, secondContentElementTypeId: string) {
    await this.ensureNameNotExists(name);
  
    const blockList = new BlockListDataTypeBuilder()
      .withName(name)
      .addBlock()
        .withContentElementTypeKey(firstContentElementTypeId)
        .done()
      .addBlock()
        .withContentElementTypeKey(secondContentElementTypeId)
        .done()
      .build();

    return await this.save(blockList);
  }
  
  async createBlockListDataTypeWithABlock(name: string, contentElementTypeId: string) {
    await this.ensureNameNotExists(name);
  
    const blockList = new BlockListDataTypeBuilder()
      .withName(name)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .done()
      .build();

    return await this.save(blockList);
  }
  
  async createBlockListDataTypeWithContentAndSettingsElementType(name: string, contentElementTypeId: string, settingsElementTypeId: string) {
    await this.ensureNameNotExists(name);
  
    const blockList = new BlockListDataTypeBuilder()
      .withName(name)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withSettingsElementTypeKey(settingsElementTypeId)
        .done()
      .build();

    return await this.save(blockList);
  }
  
  async createBlockListDataTypeWithMinAndMaxAmount(name: string, minAmount: number = 0, maxAmount: number = 0) {
    await this.ensureNameNotExists(name);
  
    const blockList = new BlockListDataTypeBuilder()
      .withName(name)
      .withMinValue(minAmount)
      .withMaxValue(maxAmount)
      .build();
    
    return await this.save(blockList);
  }

  async createBlockListDataTypeWithSingleBlockMode(name: string, enabled: boolean) {
    await this.ensureNameNotExists(name);

    const blockList = new BlockListDataTypeBuilder()
      .withName(name)
      .withSingleBlockMode(enabled)
      .build();

    return await this.save(blockList);
  }

  async createBlockListDataTypeWithLiveEditingMode(name: string, enabled: boolean) {
    await this.ensureNameNotExists(name);

    const blockList = new BlockListDataTypeBuilder()
      .withName(name)
      .withLiveEditing(enabled)
      .build();

    return await this.save(blockList);
  }
  
  async createBlockListDataTypeWithInlineEditingMode(name: string, enabled: boolean) {
    await this.ensureNameNotExists(name);
  
    const blockList = new BlockListDataTypeBuilder()
      .withName(name)
      .withInlineEditingAsDefault(enabled)
      .build();

    return await this.save(blockList);
  }
  
  async createBlockListDataTypeWithPropertyEditorWidth(name: string, width: string) {
    await this.ensureNameNotExists(name);
  
    const blockList = new BlockListDataTypeBuilder()
      .withName(name)
      .withMaxPropertyWidth(width)
      .build();

    return await this.save(blockList);
  }
  
  async createBlockListWithBlockWithEditorAppearance(name: string, elementTypeId: string, label: string = '', overlaySize: string = 'small') {
    await this.ensureNameNotExists(name);
  
    const blockList = new BlockListDataTypeBuilder()
      .withName(name)
      .addBlock()
        .withContentElementTypeKey(elementTypeId)
        .withLabel(label)
        .withEditorSize(overlaySize)
        .done()
      .build();

    return await this.save(blockList);
  }

  async createBlockListWithBlockWithCatalogueAppearance(name: string, elementTypeId: string, backgroundColor: string = '', iconColor: string = '', customStylesheet: string = '') {
    await this.ensureNameNotExists(name);
  
    const blockList = new BlockListDataTypeBuilder()
      .withName(name)
      .addBlock()
        .withContentElementTypeKey(elementTypeId)
        .withBackgroundColor(backgroundColor)
        .withIconColor(iconColor)
        .withStylesheet(customStylesheet)
        .done()
      .build();

    return await this.save(blockList);
  }
  
  async createBlockListWithBlockWithHideContentEditor(name: string, elementTypeId: string, hideContentEditor: boolean) {
    await this.ensureNameNotExists(name);
  
    const blockList = new BlockListDataTypeBuilder()
      .withName(name)
      .addBlock()
        .withContentElementTypeKey(elementTypeId)
        .withHideContentEditor(hideContentEditor)
        .done()
      .build();

    return await this.save(blockList);
  }

  async isSingleBlockModeEnabledForBlockList(blockListName: string, enabled: boolean) {
    const blockList = await this.getByName(blockListName);
    const singleBlockModeValue = blockList.values.find(value => value.alias === 'useSingleBlockMode');
    return singleBlockModeValue?.value === enabled;
  }

  async isInlineEditingModeEnabledForBlockList(blockListName: string, enabled: boolean) {
    const blockList = await this.getByName(blockListName);
    const inlineEditingModeValue = blockList.values.find(value => value.alias === 'useInlineEditingAsDefault');
    return inlineEditingModeValue?.value === enabled;
  }
  
  // Block Grid
  async createEmptyBlockGrid(blockGridName: string) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithABlock(blockGridName: string, contentElementTypeId: string) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withAllowAtRoot(true)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithABlockInAGroup(blockGridName: string, contentElementTypeId: string, groupName: string ) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlockGroup()
        .withName(groupName)
        .done()
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withGroupName(groupName)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithMinAndMaxAmount(blockGridName: string, minAmount: number = 0, maxAmount: number = 0) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .withMinValue(minAmount)
      .withMaxValue(maxAmount)
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithLiveEditingMode(blockGridName: string, enabled: boolean) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .withLiveEditing(enabled)
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithPropertyEditorWidth(blockGridName: string, width: string) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .withMaxPropertyWidth(width)
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithCreateButtonLabel(blockGridName: string, label: string = '') {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .withCreateLabel(label)
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithGridColumns(blockGridName: string, columns: number = 12) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .withGridColumns(columns)
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithLayoutStylesheet(blockGridName: string, stylesheetName: string) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .withLayoutStylesheet('/wwwroot/css/' + stylesheetName)
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithAnAreaInABlock(blockGridName: string, contentElementTypeId: string, areaAlias: string = 'area', createButtonLabel :string = '', columnSpan: number = 6, rowSpan: number = 1, minAllowed: number = 0, maxAllowed: number = 2) {
    await this.ensureNameNotExists(blockGridName);
    
    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withAllowAtRoot(true)
        .addArea()
          .withAlias(areaAlias)
          .withCreateLabel(createButtonLabel)
          .withColumnSpan(columnSpan)
          .withRowSpan(rowSpan)
          .withMinAllowed(minAllowed)
          .withMaxAllowed(maxAllowed)
          .done()
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithAnAreaInABlockWithAllowInAreas(blockGridName: string, contentElementTypeId: string, areaAlias: string = 'area', allowInAreas = false, createButtonLabel :string = 'CreateLabel', columnSpan: number = 6, rowSpan: number = 1, minAllowed: number = 0, maxAllowed: number = 2) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withAllowAtRoot(true)
        .withAllowInAreas(allowInAreas)
        .addArea()
          .withAlias(areaAlias)
          .withCreateLabel(createButtonLabel)
          .withColumnSpan(columnSpan)
          .withRowSpan(rowSpan)
          .withMinAllowed(minAllowed)
          .withMaxAllowed(maxAllowed)
          .done()
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithAnAreaInABlockWithAllowInAreasAndASecondBlock(blockGridName: string, contentElementTypeId: string, secondContentElementTypeId: string, areaAlias: string = 'area', allowInAreas = false, createButtonLabel: string = 'CreateLabel', columnSpan: number = 6, rowSpan: number = 1, minAllowed: number = 0, maxAllowed: number = 2, secondBlockAllowAtRoot: boolean = true, secondBlockAllowInAreas: boolean = true) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withAllowAtRoot(true)
        .withAllowInAreas(allowInAreas)
        .addArea()
          .withAlias(areaAlias)
          .withCreateLabel(createButtonLabel)
          .withColumnSpan(columnSpan)
          .withRowSpan(rowSpan)
          .withMinAllowed(minAllowed)
          .withMaxAllowed(maxAllowed)
          .done()
        .done()
      .addBlock()
        .withContentElementTypeKey(secondContentElementTypeId)
        .withAllowAtRoot(secondBlockAllowAtRoot)
        .withAllowInAreas(secondBlockAllowInAreas)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithAnAreaInABlockWithACreateLabel(blockGridName: string, contentElementTypeId: string, createButtonLabel: string = '', areaAlias: string = 'area') {
    return await this.createBlockGridWithAnAreaInABlockWithAllowInAreas(blockGridName, contentElementTypeId, areaAlias, true, createButtonLabel);
  }

  async createBlockGridWithAnAreaInABlockWithColumnSpanAndRowSpan(blockGridName: string, contentElementTypeId: string, columnSpan: number = 6, rowSpan: number = 1, areaAlias: string = 'area', createButtonLabel: string = 'CreateLabel') {
    return await this.createBlockGridWithAnAreaInABlockWithAllowInAreas(blockGridName, contentElementTypeId, areaAlias, true, createButtonLabel, columnSpan, rowSpan);
  }

  async createBlockGridWithAnAreaInABlockWithMinAndMaxAllowed(blockGridName: string, contentElementTypeId: string, secondContentElementTypeId: string, minAllowed: number = 0, maxAllowed: number = 2, areaAlias: string = 'area', createButtonLabel: string = 'CreateLabel') {
    return await this.createBlockGridWithAnAreaInABlockWithAllowInAreasAndASecondBlock(blockGridName, contentElementTypeId, secondContentElementTypeId, areaAlias, true, createButtonLabel, 6, 1, minAllowed, maxAllowed);
  }

  async createBlockGridWithAdvancedSettingsInBlock(blockGridName: string, contentElementTypeId: string, customViewPath: string = '', customStylesheetPath: string = '', overlaySize: string = 'small', inlineEditing: boolean = false, hideContentEditor: boolean = false) {
    await this.ensureNameNotExists(blockGridName);
    const encodedViewPath = await this.api.stylesheet.encodeStylesheetPath(customViewPath);
    const encodedStylesheetPath = await this.api.stylesheet.encodeStylesheetPath(customStylesheetPath);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withView(encodedViewPath)
        .withStylesheet(encodedStylesheetPath)
        .withEditorSize(overlaySize)
        .withInlineEditing(inlineEditing)
        .withHideContentEditor(hideContentEditor)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithCatalogueAppearanceInBlock(blockGridName: string, contentElementTypeId: string, backgroundColor: string = '', iconColor: string = '', thumbnail: string = '') {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withBackgroundColor(backgroundColor)
        .withIconColor(iconColor)
        .withThumbnail(thumbnail)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithContentAndSettingsElementType(blockGridName: string, contentElementTypeId: string, settingsElementTypeId: string, allowAtRoot: boolean = true) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withSettingsElementTypeKey(settingsElementTypeId)
        .withAllowAtRoot(allowAtRoot)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithLabel(blockGridName: string, contentElementTypeId: string, label: string, allowAtRoot: boolean = true) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withLabel(label)
        .withAllowAtRoot(allowAtRoot)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithPermissions(blockGridName: string, contentElementTypeId: string, toAllowInRoot: boolean = false, toAllowInAreas: boolean = false) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withAllowAtRoot(toAllowInRoot)
        .withAllowInAreas(toAllowInAreas)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithSizeOptions(blockGridName: string, contentElementTypeId: string, columnSpans: number = 0, minRowSpan: number = 0, maxRowSpan: number = 12) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .addColumnSpanOptions(columnSpans)
        .withMinRowSpan(minRowSpan)
        .withMaxRowSpan(maxRowSpan)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithABlockWithInlineEditingModeAndABlockWithAnArea(blockGridName: string, firstBlockElementTypeId: string, inlineEditing: boolean, secondBlockElementTypeId: string, areaAlias: string = 'area') {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(firstBlockElementTypeId)
        .withInlineEditing(inlineEditing)
        .withAllowInAreas(true)
        .done()
      .addBlock()
        .withContentElementTypeKey(secondBlockElementTypeId)
        .withAllowAtRoot(true)
        .addArea()
          .withAlias(areaAlias)
          .withColumnSpan(12)
          .withRowSpan(1)
          .addSpecifiedAllowance()
            .withElementTypeKey(firstBlockElementTypeId)
            .done()
          .done()
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async getBlockGridAreaKeyFromBlock(blockGridName: string, elementTypeKey: string, areaAlias: string) {
    const blockGrid = await this.getByName(blockGridName);
    const blocksValue = blockGrid.values.find(value => value.alias === 'blocks');

    const block = blocksValue.value.find(block => block.contentElementTypeKey === elementTypeKey);
    const area = block.areas.find(area => area.alias === areaAlias);
    return area.key;
  }

  async doesBlockEditorContainBlocksWithContentTypeIds(blockEditorName: string, elementTypeIds: string[]) {
    if (!elementTypeIds || elementTypeIds.length === 0) {
      return false;
    }

    const blockEditor = await this.getByName(blockEditorName);
    const blocksValue = blockEditor.values.find(value => value.alias === 'blocks');
    if (!blocksValue || blocksValue.value.length === 0) {
      return false;
    }

    const contentElementTypeKeys = blocksValue.value.map(block => block.contentElementTypeKey);
    return elementTypeIds.every(id => contentElementTypeKeys.includes(id));
  }

  async doesBlockEditorContainBlocksWithSettingsTypeIds(blockEditorName: string, elementTypeIds: string[]) {
    if (!elementTypeIds || elementTypeIds.length === 0) {
      return false;
    }

    const blockEditor = await this.getByName(blockEditorName);
    const blocksValue = blockEditor.values.find(value => value.alias === 'blocks');
    if (!blocksValue || blocksValue.value.length === 0) {
      return false;
    }

    const settingsElementTypeKeys = blocksValue.value.map(block => block.settingsElementTypeKey);
    return elementTypeIds.every(id => settingsElementTypeKeys.includes(id));
  }

  async isLiveEditingModeEnabledForBlockEditor(blockEditorName: string, enabled: boolean) {
    const blockEditor = await this.getByName(blockEditorName);
    const liveEditingModeValue = blockEditor.values.find(value => value.alias === 'useLiveEditing');
    return liveEditingModeValue?.value === enabled;
  }

  async doesMaxPropertyContainWidthForBlockEditor(blockEditorName: string, width: string) {
    const blockEditor = await this.getByName(blockEditorName);
    const maxPropertyWidthValue = blockEditor.values.find(value => value.alias === 'maxPropertyWidth');
    return maxPropertyWidthValue?.value === width;
  }

  async doesBlockEditorBlockContainLabel(blockName: string, elementTypeKey: string, label: string) {
    const block = await this.getBlockWithContentElementTypeId(blockName, elementTypeKey);
    return block.label === label;
  }

  async doesBlockGridGroupContainCorrectBlocks(blockGridName: string, groupName: string, elementTypeIds: string[]) {
    if (!elementTypeIds || elementTypeIds.length === 0) {
      return false;
    }

    const blockEditor = await this.getByName(blockGridName);
    // We need to get the GroupKey, so we can use it to find the blocks that use the Key.
    const blockGroupsValue = blockEditor.values.find(value => value.alias === 'blockGroups');
    if (!blockGroupsValue || blockGroupsValue.value.length === 0) {
      return false;
    }

    const blockGroupKey = blockGroupsValue.value.find(blockGroup => blockGroup.name === groupName).key;
    const blocksValue = blockEditor.values.find(value => value.alias === 'blocks');
    if (!blocksValue || blocksValue.value.length === 0) {
      return false;
    }

    const blocksWithGroupKey = blocksValue.value.filter(block => block.groupKey === blockGroupKey);
    return elementTypeIds.every(id =>
      blocksWithGroupKey.some(block => block.contentElementTypeKey === id)
    );
  }

  async doesBlockGridContainCreateButtonLabel(blockGridName: string, label: string) {
    const blockEditor = await this.getByName(blockGridName);
    const createLabelValue = blockEditor.values.find(value => value.alias === 'createLabel');
    return createLabelValue?.value === label;
  }

  async doesBlockGridContainGridColumns(blockGridName: string, columns: number) {
    const blockEditor = await this.getByName(blockGridName);
    const gridColumnsValue = blockEditor.values.find(value => value.alias === 'gridColumns');
    return gridColumnsValue?.value === columns;
  }

  async doesBlockEditorBlockHaveAllowInRootEnabled(blockGridName: string, elementTypeKey: string) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.allowAtRoot;
  }

  async doesBlockEditorBlockHaveAllowInAreasEnabled(blockGridName: string, elementTypeKey: string) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.allowInAreas;
  }

  async doesBlockEditorBlockContainColumnSpanOptions(blockGridName: string, elementTypeKey: string, expectedColumnSpans: number[]) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);

    // If the block does not have any columnSpanOptions, and we are not expecting any, return true
    if (block.columnSpanOptions.length === 0 && expectedColumnSpans.length === 0) {
      return true;
    }

    const columnSpans = block.columnSpanOptions.map(option => option.columnSpan);
    return expectedColumnSpans.every(span => columnSpans.includes(span)) && columnSpans.every(span => expectedColumnSpans.includes(span));
  }

  async doesBlockEditorBlockContainRowSpanOptions(blockGridName: string, elementTypeKey: string, minRowSpan: number, maxRowSpan: number) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.rowMinSpan === minRowSpan && block.rowMaxSpan === maxRowSpan;
  }

  async doesBlockEditorBlockContainAreaGridColumns(blockGridName: string, elementTypeKey: string, areaGridColumns: number) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.areaGridColumns === areaGridColumns;
  }

  async doesBlockEditorBlockContainAreaWithAlias(blockGridName: string, elementTypeKey: string, areaAlias: string = 'area') {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.areas.find(area => area.alias === areaAlias);
  }
  
  async doesBlockEditorBlockContainAreaCount(blockGridName: string, elementTypeKey: string, areaCount: number) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.areas.length === areaCount;
  }

  async doesBlockEditorBlockContainAreaWithCreateButtonLabel(blockGridName: string, elementTypeKey: string, areaAlias: string = 'area', createButtonLabel: string) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.areas.find(area => area.createLabel === createButtonLabel && area.alias === areaAlias);
  }

  async doesBlockEditorBlockContainAreaWithMinAllowed(blockGridName: string, elementTypeKey: string, areaAlias: string = 'area', minAllowed: number) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.areas.find(area => area.minAllowed === minAllowed && area.alias === areaAlias);
  }

  async doesBlockEditorBlockContainAreaWithMaxAllowed(blockGridName: string, elementTypeKey: string, areaAlias: string = 'area', maxAllowed: number) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.areas.find(area => area.maxAllowed === maxAllowed && area.alias === areaAlias);
  }

  async doesBlockEditorBlockContainAreaWithSpecifiedAllowance(blockGridName: string, elementTypeKey: string, areaAlias: string = 'area') {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    const area = block.areas.find(area => area.alias === areaAlias);
    return area && area.specifiedAllowance && area.specifiedAllowance.length > 0;
  }

  async doesBlockEditorBlockContainStylesheet(blockGridName: string, elementTypeKey: string, stylesheetPath: string) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    const encodedSecondStylesheetPath = await this.api.stylesheet.encodeStylesheetPath(stylesheetPath);
    return block.stylesheet[0] === encodedSecondStylesheetPath;
  }

  async doesBlockEditorBlockContainOverlaySize(blockGridName: string, elementTypeKey: string, overlaySize: string) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.editorSize === overlaySize;
  }

  async doesBlockEditorBlockContainInlineEditing(blockGridName: string, elementTypeKey: string, inlineEditing: boolean) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.inlineEditing === inlineEditing;
  }

  async doesBlockEditorBlockContainHideContentEditor(blockGridName: string, elementTypeKey: string, hideContentEditor: boolean) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.hideContentEditor === hideContentEditor;
  }

  async doesBlockEditorBlockContainBackgroundColor(blockGridName: string, elementTypeKey: string, backgroundColor: string) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.backgroundColor === backgroundColor;
  }

  async doesBlockEditorBlockContainIconColor(blockGridName: string, elementTypeKey: string, iconColor: string) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.iconColor === iconColor;
  }

  async doesBlockEditorBlockContainThumbnail(blockGridName: string, elementTypeKey: string, thumbnail: string) {
    const block = await this.getBlockWithContentElementTypeId(blockGridName, elementTypeKey);
    return block.thumbnail === thumbnail;
  }
  
  async getBlockWithContentElementTypeId(blockGridName: string, contentElementTypeKey: string) {
    const blockEditor = await this.getByName(blockGridName);
    const blocks = blockEditor.values.find(value => value.alias === 'blocks');
    return blocks.value.find(block => block.contentElementTypeKey === contentElementTypeKey);
  }

  async createImageCropperDataTypeWithOneCrop(name: string, cropLabel: string, cropWidth: number, cropHeight: number) {
    await this.ensureNameNotExists(name);

    const dataType = new ImageCropperDataTypeBuilder()
      .withName(name)
      .addCrop()
        .withLabel(cropLabel)
        .withAlias(AliasHelper.toAlias(cropLabel))
        .withHeight(cropHeight)
        .withWidth(cropWidth)
        .done()
      .build();

    return await this.save(dataType);
  }

  async createMediaPickerDataTypeWithStartNodeId(name: string, startNodeId: string) {
    await this.ensureNameNotExists(name);

    const dataType = new MediaPickerDataTypeBuilder()
      .withName(name)
      .withStartNodeId(startNodeId)
      .withIgnoreUserStartNodes(false)
      .build();

    return await this.save(dataType);
  }

  async createRadioboxDataType(name: string, options: string[] = []) {
    await this.ensureNameNotExists(name);

    const dataType = new RadioboxDataTypeBuilder()
      .withName(name)
      .withItems(options)
      .build();
      
    return await this.save(dataType);
  }

  async createImageMediaPickerDataType(name: string, minValue = 0, maxValue = 1, enableLocalFocalPoint = false, ignoreUserStartNodes = false) {
    await this.ensureNameNotExists(name);
    const mediaType = await this.api.mediaType.getByName('Image');

    const dataType = new MediaPickerDataTypeBuilder()
      .withName(name)
      .withFilter(mediaType.id)
      .withMultiple(false)
      .withMinValue(minValue)
      .withMaxValue(maxValue)
      .withEnableLocalFocalPoint(enableLocalFocalPoint)
      .withIgnoreUserStartNodes(ignoreUserStartNodes)
      .build();

    return await this.save(dataType);
  }

  async createImageMediaPickerDataTypeWithStartNodeId(name: string, startNodeId: string) {
    await this.ensureNameNotExists(name);
    const mediaType = await this.api.mediaType.getByName('Image');

    const dataType = new MediaPickerDataTypeBuilder()
      .withName(name)
      .withFilter(mediaType.id)
      .withStartNodeId(startNodeId)
      .build();

    return await this.save(dataType);
  }
  
  async createImageMediaPickerDataTypeWithCrop(name: string, label: string, width: number, height: number) {
    await this.ensureNameNotExists(name);
    const mediaType = await this.api.mediaType.getByName('Image');

    const dataType = new MediaPickerDataTypeBuilder()
      .withName(name)
      .withFilter(mediaType.id)
      .addCrop()
        .withLabel(label)
        .withAlias(AliasHelper.toAlias(label))
        .withHeight(height)
        .withWidth(width)
        .done()
      .build();

    return await this.save(dataType);
  }

  async createTextareaDataType(name: string, maxChars: number = 0, rows: number = 0, minHeight: number = 0, maxHeight: number = 0) {
    await this.ensureNameNotExists(name);

    const dataType = new TextAreaDataTypeBuilder()
      .withName(name)
      .withMaxChars(maxChars)
      .withRows(rows)
      .withMinHeight(minHeight)
      .withMaxHeight(maxHeight)
      .build();

    return await this.save(dataType);
  }

  async createTextstringDataType(name: string, maxChars: number = 50) {
    await this.ensureNameNotExists(name);

    const dataType = new TextStringDataTypeBuilder()
      .withName(name)
      .withMaxChars(maxChars)
      .build();

    return await this.save(dataType);
  }
  
  async createTrueFalseDataTypeWithInitialState(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new TrueFalseDataTypeBuilder()
      .withName(name)
      .withIsDefault(true)
      .build();

    return await this.save(dataType);
  }

  async createTrueFalseDataTypeWithLabelOn(name: string, labelOn: string) {
    await this.ensureNameNotExists(name);

    const dataType = new TrueFalseDataTypeBuilder()
      .withName(name)
      .withShowLabels(true)
      .withLabelOn(labelOn)
      .build();

    return await this.save(dataType);
  }

  async createTrueFalseDataTypeWithLabelOff(name: string, labelOff: string) {
    await this.ensureNameNotExists(name);

    const dataType = new TrueFalseDataTypeBuilder()
      .withName(name)
      .withShowLabels(true)
      .withLabelOff(labelOff)
      .build();

    return await this.save(dataType);
  }

  async createEmailAddressDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new EmailAddressDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async createCodeEditorDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new CodeEditorDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async createMarkdownEditorDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new MarkdownEditorDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async createDecimalDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new DecimalDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async createMultipleTextStringDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new MultipleTextStringDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async createSliderDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new SliderDataTypeBuilder()
      .withName(name)
      .withMaxValue(100)
      .withStep(1)
      .build();

    return await this.save(dataType);
  }

  // List View - Content data type
  async createListViewContentDataType(name: string = 'List View - Content Test') {
    await this.ensureNameNotExists(name);

    const dataType = new ListViewDataTypeBuilder()
      .withName(name)
      .withPageSize(100)
      .addLayout()
        .withName('List')
        .withIcon('icon-list')
        .withCollectionView('Umb.CollectionView.Document.Table')
        .done()
      .addLayout()
        .withName('Grid')
        .withIcon('icon-grid')
        .withCollectionView('Umb.CollectionView.Document.Grid')
        .done()
      .addColumnDisplayedProperty()
        .withAlias('updateDate')
        .withHeader('Last edited')
        .withIsSystem(true)
        .done()
      .addColumnDisplayedProperty()
        .withAlias('creator')
        .withHeader('Updated by')
        .withIsSystem(true)
        .done()
      .build();
    return await this.save(dataType);
  }

  async createListViewContentDataTypeWithLayoutAndPageSize(name: string = 'List View - Content Test', layoutAlias: string = 'Umb.CollectionView.Document.Table', layoutName: string = 'List', pageSize: number = 100 ) {
    await this.ensureNameNotExists(name);

    const dataType = new ListViewDataTypeBuilder()
      .withName(name)
      .withPageSize(pageSize)
      .withOrderDirection('asc')
      .addLayout()
        .withName(layoutName)
        .withIcon('icon-list')
        .withCollectionView(layoutAlias)
        .done()
      .addColumnDisplayedProperty()
        .withAlias('updateDate')
        .withHeader('Last edited')
        .withIsSystem(true)
        .done()
      .addColumnDisplayedProperty()
        .withAlias('creator')
        .withHeader('Updated by')
        .withIsSystem(true)
        .done()
      .build();
    return await this.save(dataType);
  }

  async createListViewContentDataTypeWithAllPermissions(name: string = 'List View - Content Test') {
    await this.ensureNameNotExists(name);

    const dataType = new ListViewDataTypeBuilder()
      .withName(name)
      .withPageSize(100)
      .addLayout()
        .withName('List')
        .withIcon('icon-list')
        .withCollectionView('Umb.CollectionView.Document.Table')
        .done()
      .addLayout()
        .withName('Grid')
        .withIcon('icon-grid')
        .withCollectionView('Umb.CollectionView.Document.Grid')
        .done()
      .addColumnDisplayedProperty()
        .withAlias('updateDate')
        .withHeader('Last edited')
        .done()
      .addColumnDisplayedProperty()
        .withAlias('creator')
        .withHeader('Updated by')
        .done()
      .addBulkActionPermissions()
        .withAllowBulkCopy(true)
        .withAllowBulkDelete(true)
        .withAllowBulkMove(true)
        .withAllowBulkPublish(true)
        .withAllowBulkUnPublish(true)
        .done()
      .build();
    return await this.save(dataType);
  }
  
  // List View - Media data type
  async updateListViewMediaDataType(alias: string, newValue: any) {
    const listViewMediaData = await this.getByName('List View - Media');
    const valueData = listViewMediaData.values.find(value => value.alias === alias);
    if (valueData) {
      valueData.value = newValue;
    } else {
      listViewMediaData.values.push({
        "alias": alias,
        "value": newValue
      });
    }
    return await this.update(listViewMediaData.id, listViewMediaData);
  }

  async createDefaultTiptapDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new TiptapDataTypeBuilder()
      .withName(name)
      .build();
    
    return await this.save(dataType);
  }
  
  async createTipTapDataTypeWithABlock(name: string, contentElementTypeKey: string) {
    await this.ensureNameNotExists(name);

    const dataType = new TiptapDataTypeBuilder()
      .withName(name)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeKey)
        .done()
      .addToolbarRow()
        .addToolbarGroup()
          .withBlockPicker(true)
          .done()
        .done()
      .addExtension()
        .withBlock(true)
        .done()
      .build();
    
    return await this.save(dataType);
  };

  async createApprovedColorDataTypeWithOneItem(name: string, itemLabel: string, itemValue: string) {
    await this.ensureNameNotExists(name);

    const dataType = new ApprovedColorDataTypeBuilder()
      .withName(name)
      .withUseLabel(true)
      .addItem()
        .withLabel(itemLabel)
        .withValue(itemValue)
        .done()
      .build();

    return await this.save(dataType);
  }

  async getTiptapExtensionsCount(tipTapName: string) {
    const tipTapData = await this.getByName(tipTapName);
    const extensionsValue = tipTapData.values.find(value => value.alias === 'extensions');
    return extensionsValue?.value.length;
  }

  async getTiptapToolbarGroupInRowCount(tipTapName: string, rowIndex: number = 0) {
    const tipTapData = await this.getByName(tipTapName);
    const toolbarValue = tipTapData.values.find(value => value.alias === 'toolbar');
    return toolbarValue?.value[rowIndex].length;
  }

  async getTiptapToolbarGroupValueInRow(tipTapName: string, groupIndex: number, rowIndex: number = 0) {
    const tipTapData = await this.getByName(tipTapName);
    const toolbarValue = tipTapData.values.find(value => value.alias === 'toolbar');
    return toolbarValue?.value[rowIndex][groupIndex];
  }

  async getTiptapToolbarRowCount(tipTapName: string) {
    const tipTapData = await this.getByName(tipTapName);
    const toolbarValue = tipTapData.values.find(value => value.alias === 'toolbar');
    return toolbarValue?.value.length;
  }

  async createDefaultTinyMCEDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new TinyMCEDataTypeBuilder()
      .withName(name)
      .addToolbar()
        .withStyles(true)
        .withBold(true)
        .withItalic(true)
        .withAlignLeft(true)
        .withAlignCenter(true)
        .withAlignRight(true)
        .withBulList(true)
        .withNumList(true)
        .withOutdent(true)
        .withIndent(true)
        .withSourceCode(true)
        .withLink(true)
        .withUmbEmbedDialog(true)
        .withUmbMediaPicker(true)
        .done()
      .withEditorMode('Classic')
      .withMaxImageSize(500)
      .build();

    return await this.save(dataType);
  }

  async getTinyMCEToolbarItemsCount(tinyMCEName: string) {
    const tinyMCEData = await this.getByName(tinyMCEName);
    const toolbarValue = tinyMCEData.values.find(value => value.alias === 'toolbar');
    return toolbarValue?.value.length;
  }

  async doesTinyMCEToolbarItemsMatchCount(tinyMCEName: string, count: number) {
    const tinyMCEData = await this.getByName(tinyMCEName);
    const toolbarValue = tinyMCEData.values.find(value => value.alias === 'toolbar');
    return toolbarValue?.value.length === count;
  }

  async doesTinyMCEToolbarHaveItems(tinyMCEName: string, items: string[]) {
    const tinyMCEData = await this.getByName(tinyMCEName);
    const toolbarValue = tinyMCEData.values.find(value => value.alias === 'toolbar');
    if (!toolbarValue || toolbarValue.value.length === 0) {
      return false;
    }
    return items.every(item => toolbarValue.value.includes(item));
  }

  async createTiptapDataTypeWithTwoToolbarRows(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new TiptapDataTypeBuilder()
      .withName(name)
      .addToolbarRow()
        .addToolbarGroup()
          .withHeading1(true)
          .withHeading2(true)
          .withHeading3(true)
          .done()
        .addToolbarGroup()
          .withBold(true)
          .withItalic(true)
          .done()
        .done()
      .addToolbarRow()
        .addToolbarGroup()
          .withBlockquote(true)
          .withBulletList(true)
          .done()
        .done()
      .build();
    
    return await this.save(dataType);
  }

  async createTinyMCEDataTypeWithStylesheet(name: string, stylesheetPath: string) {
    await this.ensureNameNotExists(name);

    const dataType = new TinyMCEDataTypeBuilder()
      .withName(name)
      .addStylesheet(stylesheetPath)
      .addToolbar()
        .withBold(true)
        .withItalic(true)
        .done()
      .build();

    return await this.save(dataType);
  }
  
  async createBlockGridWithABlockAndAllowAtRoot(blockGridName: string, contentElementTypeId: string, allowAtRoot: boolean = true) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withAllowAtRoot(allowAtRoot)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithABlockAndMinAndMaxAmount(blockGridName: string, contentElementTypeId: string, minAmount: number = 0, maxAmount: number = 0) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .withMinValue(minAmount)
      .withMaxValue(maxAmount)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withAllowAtRoot(true)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithABlockAndCreateButtonLabel(blockGridName: string, contentElementTypeId: string, label: string = '') {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .withCreateLabel(label)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withAllowAtRoot(true)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockGridWithABlockAndGridColumns(blockGridName: string, contentElementTypeId: string, gridColumns: number) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .withGridColumns(gridColumns)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withAllowAtRoot(true)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createBlockListWithABlockAndMinAndMaxAmount(blockListName: string, contentElementTypeId: string, minAmount: number = 0, maxAmount: number = 0) {
    await this.ensureNameNotExists(blockListName);

    const blockList = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .withMinValue(minAmount)
      .withMaxValue(maxAmount)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .done()
      .build();

    return await this.save(blockList);
  }

  async createBlockListDataTypeWithLabel(blockListName: string, contentElementTypeId: string, label: string) {
    await this.ensureNameNotExists(blockListName);

    const blockList = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withLabel(label)
        .done()
      .build();

    return await this.save(blockList);
  }
  
  async doesBlockGridContainLayoutStylesheet(blockGridName: string, stylesheetName: string) {
    const blockEditor = await this.getByName(blockGridName);
    const layoutStylesheetValue = blockEditor.values.find(value => value.alias === 'layoutStylesheet');
    return layoutStylesheetValue?.value === '/wwwroot/css/' + stylesheetName;
  }
  
  async createRichTextEditorWithABlock(richTextEditorName: string, contentElementTypeId: string) {
    await this.ensureNameNotExists(richTextEditorName);

    const richTextEditor = new TiptapDataTypeBuilder()
      .withName(richTextEditorName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .done()
      .addExtension()
      .withBlock(true)
      .done()
      .addToolbarRow()
        .addToolbarGroup()
          .withBlockPicker(true)
          .done()
        .done()
      .build();

    return await this.save(richTextEditor);
  }

  async createRichTextEditorWithABlockWithBlockSettings(richTextEditorName: string, contentElementTypeId: string, label: string = "", backgroundColor: string = "", iconColor: string = "", thumbnail: string = "", editorSize: string = "", settingsElementTypeId: string = "", displayInline: boolean = false) {
    await this.ensureNameNotExists(richTextEditorName);

    const richTextEditor = new TiptapDataTypeBuilder()
      .withName(richTextEditorName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withLabel(label)
        .withEditorSize(editorSize)
        .withSettingsElementTypeKey(settingsElementTypeId)
        .withBackgroundColor(backgroundColor)
        .withIconColor(iconColor)
        .withThumbnail(thumbnail)
        .withDisplayInline(displayInline)
        .done()
      .addExtension()
        .withBlock(true)
        .done()
      .addToolbarRow()
        .addToolbarGroup()
          .withBlockPicker(true)
          .done()
        .done()
      .build();

    return await this.save(richTextEditor);
  }
  
  async createRichTextEditorWithABlockWithBlockSettingEditorSize(richTextEditorName: string, contentElementTypeId: string, editorSize: string) {
    return await this.createRichTextEditorWithABlockWithBlockSettings(richTextEditorName, contentElementTypeId, "", "", "", "", editorSize);
  }
  
  async createRichTextEditorWithABlockWithBlockSettingLabel(richTextEditorName: string, contentElementTypeId: string, label: string) {
    return await this.createRichTextEditorWithABlockWithBlockSettings(richTextEditorName, contentElementTypeId, label);
  }
  
  async createRichTextEditorWithABlockWithBlockSettingBackgroundColor(richTextEditorName: string, contentElementTypeId: string, backgroundColor: string) {
    return await this.createRichTextEditorWithABlockWithBlockSettings(richTextEditorName, contentElementTypeId, "", backgroundColor);
  }
  
  async createRichTextEditorWithABlockWithBlockSettingIconColor(richTextEditorName: string, contentElementTypeId: string, iconColor: string) {
    return await this.createRichTextEditorWithABlockWithBlockSettings(richTextEditorName, contentElementTypeId, "", "", iconColor);
  }
  
  async createRichTextEditorWithABlockWithBlockSettingThumbnail(richTextEditorName: string, contentElementTypeId: string, thumbnail: string) {
    return await this.createRichTextEditorWithABlockWithBlockSettings(richTextEditorName, contentElementTypeId, "", "", "", thumbnail);
  }
  
  async createRichTextEditorWithABlockWithBlockSettingSettingsElementTypeKey(richTextEditorName: string, contentElementTypeId: string, settingsElementTypeId: string) {
    return await this.createRichTextEditorWithABlockWithBlockSettings(richTextEditorName, contentElementTypeId, "", "", "", "", "", settingsElementTypeId);
  }
  
  async createRichTextEditorWithABlockWithBlockSettingDisplayInline(richTextEditorName: string, contentElementTypeId: string, displayInline: boolean) {
    return await this.createRichTextEditorWithABlockWithBlockSettings(richTextEditorName, contentElementTypeId, "", "", "", "", "", "", displayInline);
  }

  async doesDataTypeHaveValue(dataTypeName: string, alias: string, value?: any, dataTypeData?) {
    const dataType = dataTypeData || await this.getByName(dataTypeName);
    const valueData = dataType.values.find(item => item.alias === alias);
    if (!valueData) {
      return false;
    }
    if (Array.isArray(valueData?.value) && Array.isArray(value)) {
      return value.every(item => valueData.value.includes(item));
    }
    return valueData?.value === value;
  }

  async doesApprovedColorHaveColor(dataTypeName: string, color: string) {
    const dataTypeData = await this.getByName(dataTypeName);
    const valueData = dataTypeData.values.find(item => item.alias === 'items');
    return valueData?.value.find(item => item.value === color);
  }

  async createUploadDataType(name: string, fileExtensions: string[] = []) {
    await this.ensureNameNotExists(name);

    const dataType = new UploadFieldDataTypeBuilder()
      .withName(name)
      .withFileExtensions(fileExtensions)
      .build();

    return await this.save(dataType);
  }

  async createDefaultApprovedColorDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new ApprovedColorDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async createDefaultContentPickerDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new ContentPickerDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async createDefaultDropdownDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new DropdownDataTypeBuilder()
      .withName(name)
      .build();
    return await this.save(dataType);
  }

  async createDefaultLabelDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new LabelDataTypeBuilder()
      .withName(name)
      .build();
    return await this.save(dataType);
  }

  async createDefaultMediaPickerDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new MediaPickerDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async doesMediaPickerHaveMinAndMaxAmount(dataTypeName: string, min: number, max: number) {
    const dataTypeData = await this.getByName(dataTypeName);
    const valueData = dataTypeData.values.find(item => item.alias === 'validationLimit');
    return valueData?.value.max === max && valueData?.value.min === min;
  }

  async doesDataTypeHaveCrops(dataTypeName: string, cropLabel: string, cropAlias: string, cropWidth: number, cropHeight: number) {
    const dataTypeData = await this.getByName(dataTypeName);
    const valueData = dataTypeData.values.find(item => item.alias === 'crops');
    return valueData?.value.find(item => item.label === cropLabel && item.alias === cropAlias && item.width === cropWidth && item.height === cropHeight);
  }

  async doesRTEHaveDimensions(dataTypeName: string, width: number, height: number) {
    const dataTypeData = await this.getByName(dataTypeName);
    const valueData = dataTypeData.values.find(item => item.alias === 'dimensions');
    return valueData?.value.width === width && valueData?.value.height === height;
  }

  async doesTiptapExtensionsItemsMatchCount(tiptapName: string, count: number) {
    const tiptapData = await this.getByName(tiptapName);
    const extensionsValue = tiptapData.values.find(value => value.alias === 'extensions');
    return extensionsValue?.value.length === count;
  }

  async doesTiptapExtensionsHaveItems(tiptapName: string, items: string[]) {
    const tiptapData = await this.getByName(tiptapName);
    const extensionsValue = tiptapData.values.find(value => value.alias === 'extensions');
    if (!extensionsValue || extensionsValue.value.length === 0) {
      return false;
    }
    return items.every(item => extensionsValue.value.includes(item));
  }

  async doesTiptapToolbarHaveItems(tiptapName: string, items) {
    const tiptapData = await this.getByName(tiptapName);
    const toolbarValue = tiptapData.values.find(value => value.alias === 'toolbar');
    if (!toolbarValue || toolbarValue.value.length === 0) {
      return false;
    }
    return JSON.stringify(toolbarValue.value) === JSON.stringify(items);
  }

  async doesRTEContainBlocks(dataTypeName: string, elementTypeIds: string[]) {
    const dataTypeData = await this.getByName(dataTypeName);
    const blocksValue = dataTypeData.values.find(value => value.alias === 'blocks');
    if (!blocksValue || blocksValue.value.length === 0) {
      return false;
    }
    const contentElementTypeKeys = blocksValue.value.map(block => block.contentElementTypeKey);
    return elementTypeIds.every(id => contentElementTypeKeys.includes(id));
  }

  async createDefaultImageCropperDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new ImageCropperDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async doesListViewHaveProperty(dataTypeName: string, header: string, alias: string, isSystem: any = true) {
    const dataTypeData = await this.getByName(dataTypeName);
    const valueData = dataTypeData.values.find(item => item.alias === 'includeProperties');
    if (!valueData || valueData.value.length === 0) {
      return false;
    }
    return valueData?.value.find(item => item.header === header && item.alias === alias && item.isSystem === isSystem);
  }

  async doesListViewHaveLayout(dataTypeName: string, name: string, icon: string, collectionView: string) {
    const dataTypeData = await this.getByName(dataTypeName);
    const valueData = dataTypeData.values.find(item => item.alias === 'layouts');
    if (!valueData || valueData.value.length === 0) {
      return false;
    }
    return valueData?.value.find(item => item.name === name && item.icon === icon && item.collectionView === collectionView);
  }

  async createDefaultMultiUrlPickerDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new MultiUrlPickerDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async createDefaultNumericDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new NumericDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async createDefaultTagsDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new TagsDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async createDefaultTrueFalseDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new TrueFalseDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async createBlockListDataTypeWithInlineEditingModeAndABlock(blockListName: string, contentElementTypeId: string, inlineEditing: boolean = true) {
    await this.ensureNameNotExists(blockListName);
  
    const blockList = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .withInlineEditingAsDefault(inlineEditing)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .done()
      .build();

    return await this.save(blockList);
  }

  async createBlockGridWithABlockWithInlineEditingMode(blockGridName: string, contentElementTypeId: string, inlineEditing: boolean = true) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withInlineEditing(inlineEditing)
        .withAllowAtRoot(true)
        .withAllowInAreas(true)
        .done()
      .build();

    return await this.save(blockGrid);
  }

  async createTiptapDataTypeWithWordCountStatusbar(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new TiptapDataTypeBuilder()
      .withName(name)
      .addExtension()
        .withWordCount(true)
        .done()
      .addStatusbar()
        .withWordCount(true)
        .done()
      .build();
    
    return await this.save(dataType);
  }

  async createTiptapDataTypeWithElementPathStatusbar(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new TiptapDataTypeBuilder()
      .withName(name)
      .addStatusbar()
        .withElementPath(true)
        .done()
      .build();
    
    return await this.save(dataType);
  }

  async createTiptapDataTypeWithStyleSelect(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new TiptapDataTypeBuilder()
      .withName(name)
      .addToolbarRow()
        .addToolbarGroup()
          .withStyleSelect(true)
          .done()
        .done()
      .build();
    
    return await this.save(dataType);
  }

  async createMultiNodeTreePickerDataTypeWithAllowedTypes(name: string, allowedTypeIds: string, startNodeType: string = 'content') {
    await this.ensureNameNotExists(name);

    const dataType = new MultiNodeTreePickerDataTypeBuilder()
      .withName(name)
      .withFilterIds(allowedTypeIds)
      .addStartNode()
        .withType(startNodeType)
        .done()
      .build();
      
    return await this.save(dataType);
  }

  async createDefaultDateTimeWithTimeZonePickerDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new DateTimeWithTimeZonePickerDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async doesDateTimeWithTimeZoneHaveMode(dataTypeName: string, mode: string) {
    const dataTypeData = await this.getByName(dataTypeName);
    const valueData = dataTypeData.values.find(item => item.alias === 'timeZones');
    return valueData?.value.mode === mode;
  }

  async doesDateTimeWithTimeZoneHaveTimeZones(dataTypeName: string, timeZones: string[]) {
    const dataTypeData = await this.getByName(dataTypeName);
    const timeZonesData = dataTypeData.values.find(item => item.alias === 'timeZones');
    if (!timeZonesData || !timeZonesData.value) {
      return false;
    }
    const existingZones = timeZonesData.value.timeZones;
    return timeZones.every(timeZone => existingZones.includes(timeZone));
  }
  
  // Entity Data Picker
  async createEntityDataPickerDataType(name: string, dataSource: string) {
    await this.ensureNameNotExists(name);

    const dataType = new EntityDataPickerDataTypeBuilder()
      .withName(name)
      .withDataSource(dataSource)
      .build();

    return await this.save(dataType);
  }

  async createEntityDataPickerDataTypeWithMinAndMaxValues(name: string, dataSource: string, min: number, max: number) {
    await this.ensureNameNotExists(name);

    const dataType = new EntityDataPickerDataTypeBuilder()
      .withName(name)
      .withDataSource(dataSource)
      .withMinValue(min)
      .withMaxValue(max)
      .build();

    return await this.save(dataType);
  }
  
  async createBlockGridWithAThumbnail(blockGridName: string, contentElementTypeId: string, thumbnailPath: string) {
    await this.ensureNameNotExists(blockGridName);

    const blockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withThumbnail(thumbnailPath)
        .withAllowAtRoot(true)
        .done()
      .build();

    return await this.save(blockGrid);
  }
  
  async createBlockListWithAThumbnail(blockListName: string, contentElementTypeId: string, thumbnailPath: string) {
    await this.ensureNameNotExists(blockListName);

    const blockList = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(contentElementTypeId)
        .withThumbnail(thumbnailPath)
        .done()
      .build();

    return await this.save(blockList);
  }

  async createDefaultContentPickerSourceDataType(name: string) {
    await this.ensureNameNotExists(name);

    const dataType = new MultiNodeTreePickerDataTypeBuilder()
      .withName(name)
      .build();

    return await this.save(dataType);
  }

  async createContentPickerSourceDataTypeWithDynamicRoot(name: string, originAlias: string) {
    await this.ensureNameNotExists(name);

    const dataType = new MultiNodeTreePickerDataTypeBuilder()
      .withName(name)
      .addStartNode()
        .withType('content')
        .withOriginAlias(originAlias)
        .done()
      .build();

    return await this.save(dataType);
  }

  async doesContentPickerHaveDynamicRoot(dataTypeName: string, originAlias: string) {
    const dataType = await this.getByName(dataTypeName);
    const startNodeValue = dataType.values.find((item: any) => item.alias === 'startNode');
    if (!startNodeValue?.value?.dynamicRoot) {
      return false;
    }
    return startNodeValue.value.dynamicRoot.originAlias === originAlias;
  }

  async getContentPickerDynamicRoot(dataTypeName: string) {
    const dataType = await this.getByName(dataTypeName);
    const startNodeValue = dataType.values.find((item: any) => item.alias === 'startNode');
    return startNodeValue?.value?.dynamicRoot;
  }
}
