import { BehaviorSubject, map, Observable } from 'rxjs';

export interface DocumentNode {
  id: string;
  key: string;
  name: string;
  alias: string;
  icon: string; // TODO: should come from the doc type?
  properties: NodeProperty[];
  data: any; // TODO: define data type
  layout?: any; // TODO: define layout type - make it non-optional
}

export interface NodeProperty {
  alias: string;
  label: string;
  description: string;
  dataTypeAlias: string;
  tempValue: string; // TODO: remove this - only used for testing
}

export const data: Array<DocumentNode> = [
  {
    id: '1',
    key: '74e4008a-ea4f-4793-b924-15e02fd380d3',
    name: 'Document 1',
    alias: 'document1',
    icon: 'document',
    properties: [
      {
        alias: 'myHeadline',
        label: 'Textarea label',
        description: 'this is a textarea property',
        dataTypeAlias: 'myTextStringEditor',
        tempValue: 'hello world 1'
      },
      {
        alias: 'myDescription',
        label: 'Text string label',
        description: 'This is the a text string property',
        dataTypeAlias: 'myTextAreaEditor',
        tempValue: 'Tex areaaaa 1'
      },
    ],
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
    /*
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
    id: '2',
    key: '74e4008a-ea4f-4793-b924-15e02fd380d3',
    name: 'Document 2',
    alias: 'document2',
    icon: 'favorite',
    properties: [
      {
        alias: 'myHeadline',
        label: 'Textarea label',
        description: 'this is a textarea property',
        dataTypeAlias: 'myTextStringEditor',
        tempValue: 'hello world 2'
      },
      {
        alias: 'myDescription',
        label: 'Text string label',
        description: 'This is the a text string property',
        dataTypeAlias: 'myTextAreaEditor',
        tempValue: 'Tex areaaaa 2'
      },
    ],
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
    /*
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
  }
];

export class UmbContentService {

  private _nodes: BehaviorSubject<Array<DocumentNode>> = new BehaviorSubject(<Array<DocumentNode>>[]);
  public readonly nodes: Observable<Array<DocumentNode>> = this._nodes.asObservable();

  constructor () {
    this._nodes.next(data);
  }

  getById (id: string): Observable<DocumentNode | null> {
    return this.nodes.pipe(map(((nodes: Array<DocumentNode>) => nodes.find((node: DocumentNode) => node.id === id) || null)));
  }

}