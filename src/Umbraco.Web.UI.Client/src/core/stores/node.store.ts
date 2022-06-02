import { BehaviorSubject, map, Observable } from 'rxjs';
import { DocumentNode } from '../../mocks/data/content.data';

export class UmbNodeStore {
  private _nodes: BehaviorSubject<Array<DocumentNode>> = new BehaviorSubject(<Array<DocumentNode>>[]);
  public readonly nodes: Observable<Array<DocumentNode>> = this._nodes.asObservable();

  getById(id: number): Observable<DocumentNode | null> {
    // fetch from server and update store
    fetch(`/umbraco/backoffice/content/${id}`)
      .then((res) => res.json())
      .then((data) => {
        this._updateStore(data);
      });

    return this.nodes.pipe(
      map((nodes: Array<DocumentNode>) => nodes.find((node: DocumentNode) => node.id === id) || null)
    );
  }

  // TODO: Use Node type, to not be specific about Document.
  // TODO: make sure UI somehow can follow the status of this action.
  save(data: DocumentNode[]) {
    // fetch from server and update store
    // TODO: use Fetcher API.
    let body: string;

    try {
      body = JSON.stringify(data);
    } catch (error) {
      console.error(error);
      return;
    }

    // TODO: Use node type to hit the right API, or have a general Node API?
    fetch('/umbraco/backoffice/content/save', {
      method: 'POST',
      body: body,
      headers: {
        'Content-Type': 'application/json',
      },
    })
      .then((res) => res.json())
      .then((data) => {
        this._updateStore(data);
      });
  }

  private _updateStore(fetchedNodes: Array<any>) {
    const storedNodes = this._nodes.getValue();
    const updated: DocumentNode[] = [...storedNodes];

    fetchedNodes.forEach((fetchedNode) => {
      const index = storedNodes.map((storedNode) => storedNode.id).indexOf(fetchedNode.id);

      if (index !== -1) {
        // If the node is already in the store, update it
        updated[index] = fetchedNode;
      } else {
        // If the node is not in the store, add it
        updated.push(fetchedNode);
      }
    });

    this._nodes.next([...updated]);
  }
}
