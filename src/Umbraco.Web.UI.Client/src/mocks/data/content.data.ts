export interface DocumentNode {
  id: number;
  key: string;
  name: string;
  alias: string;
  icon: string; // TODO: should come from the doc type?
  properties: NodeProperty[];
  //data: any; // TODO: define data type
  //layout?: any; // TODO: define layout type - make it non-optional
}

export interface DataTypeEntity {
  id: number;
  key: string;
  name: string;
  //icon: string; // TODO: should come from the doc type?
  //configUI: any; // this is the prevalues...
  propertyEditorUIAlias: string;
}

export interface NodeProperty {
  alias: string;
  label: string;
  description: string;
  dataTypeKey: string;
  tempValue: string; // TODO: remove this - only used for testing
}

export const data: Array<DocumentNode> = [
  {
    id: 1,
    key: '74e4008a-ea4f-4793-b924-15e02fd380d1',
    name: 'Document 1',
    alias: 'document1',
    icon: 'document',
    properties: [
      {
        alias: 'myHeadline',
        label: 'Textarea label 1',
        description: 'this is a textarea property',
        dataTypeKey: 'dt-1',
        tempValue: 'hello world 1',
      },
      {
        alias: 'myDescription',
        label: 'Text string label 1',
        description: 'This is the a text string property',
        dataTypeKey: 'dt-2',
        tempValue: 'Tex areaaaa 1',
      },
    ],
    /*
    // Concept for stored values, better approach for variants, separating data from structure/configuration
    data: [
      {
        alias: 'myHeadline',
        value: 'hello world',
      },
      {
        alias: 'myDescription',
        value: 'Teeeeexxxt areaaaaaa',
      },
    ],
    */
    /*
    // Concept for node layout, separation of design from config and data.
    layout: [
      {
        type: 'group',
        children: [
          {
            type: 'property',
            alias: 'myHeadline'
          },
          {
            type: 'property',
            alias: 'myDescription'
          }
        ]
      }
    ],
    */
  },
  {
    id: 2,
    key: '74e4008a-ea4f-4793-b924-15e02fd380d2',
    name: 'Document 2',
    alias: 'document2',
    icon: 'favorite',
    properties: [
      {
        alias: 'myHeadline',
        label: 'Textarea label 2',
        description: 'this is a textarea property',
        dataTypeKey: 'dt-1',
        tempValue: 'hello world 2',
      },
      {
        alias: 'myDescription',
        label: 'Text string label 2',
        description: 'This is the a text string property',
        dataTypeKey: 'dt-2',
        tempValue: 'Tex areaaaa 2',
      },
      {
        alias: 'myExternalEditor',
        label: 'External label 1',
        description: 'This is the a external property',
        dataTypeKey: 'dt-3',
        tempValue: 'Tex lkasdfkljdfsa 1',
      },
      {
        alias: 'myContextExampleEditor',
        label: 'Context example label 1',
        description: 'This is the a example property',
        dataTypeKey: 'dt-4',
        tempValue: '',
      },
    ],
  },
];

// Temp mocked database
class UmbContentData {
  private _data: Array<DocumentNode> = [];

  constructor() {
    this._data = data;
  }

  getById(id: number) {
    return this._data.find((item) => item.id === id);
  }

  save(nodes: DocumentNode[]) {
    nodes.forEach((node) => {
      const foundIndex = this._data.findIndex((item) => item.id === node.id);
      if (foundIndex !== -1) {
        // replace
        this._data[foundIndex] = node;
      } else {
        // new
        this._data.push(node);
      }
    });
    //console.log('save:', nodes);
    return nodes;
  }
}

export const umbContentData = new UmbContentData();
