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

/* TODO:
Consider splitting data into smaller thunks that matches our different stores.
example: we need an entity store for things in the tree, so we dont load the full nodes for everything in the tree.
We would like the tree items to stay up to date, without requesting the server again.

If we split entityData into its own object, then that could go in the entityStore and be merged with the nodeStore (we would have a subscription on both).
*/
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
        label: 'Headline',
        description: 'Text string property',
        dataTypeKey: 'dt-1',
        tempValue: 'The daily life at Umbraco HQ',
      },
      {
        alias: 'myDescription',
        label: 'Description',
        description: 'Textarea property',
        dataTypeKey: 'dt-2',
        tempValue: 'Every day, a rabbit in a military costume greets me at the front door',
      },
    ],
    /*
    // Concept for stored values, better approach for variants, separating data from structure/configuration, still needs structure for variants. (We could actually split it up so we have each variants data through a separate end-point?)
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
        label: 'Text string label',
        description: 'this is a text string property',
        dataTypeKey: 'dt-1',
        tempValue: 'Is it all just fun and curling and scary rabbits?',
      },
      {
        alias: 'myDescription',
        label: 'Textarea label',
        description: 'This is the a textarea property',
        dataTypeKey: 'dt-2',
        tempValue: 'So no, there\'s not confetti every day. And no, there\'s not champagne every week or a crazy rabbit running around 🐰',
      },
      {
        alias: 'myExternalEditor',
        label: 'My JS Property Editor',
        description: 'This is the a external property',
        dataTypeKey: 'dt-3',
        tempValue: 'Tex lkasdfkljdfsa 1',
      },
      {
        alias: 'myContextExampleEditor',
        label: 'Context example label',
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
