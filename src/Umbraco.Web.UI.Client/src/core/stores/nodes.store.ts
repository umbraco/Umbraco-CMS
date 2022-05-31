import { BehaviorSubject, map, Observable } from 'rxjs';
import { DocumentNode } from '../../mocks/data/content.data';

export class UmbNodesStore {

  private _nodes: BehaviorSubject<Array<DocumentNode>> = new BehaviorSubject(<Array<DocumentNode>>[]);
  public readonly nodes: Observable<Array<DocumentNode>> = this._nodes.asObservable();

  getById (id: number): Observable<DocumentNode | null> {
    // fetch from server and update store
    fetch(`/umbraco/backoffice/content/${id}`)
      .then(res => res.json())
      .then(data => {
        this._updateStore(data);
      });

    return this.nodes.pipe(map(((nodes: Array<DocumentNode>) => nodes.find((node: DocumentNode) => node.id === id) || null)));
  }

  private _updateStore (fetchedNodes: Array<any>) {
    const storedNodes = this._nodes.getValue();
    let updated: any = [...storedNodes];

    fetchedNodes.forEach(fetchedNode => {
      const index = storedNodes.map(storedNode => storedNode.id).indexOf(fetchedNode.id);

      if (index !== -1) {
        // If the node is already in the store, update it
        updated[index] = fetchedNode;
      } else {
        // If the node is not in the store, add it
        updated = [...updated, fetchedNode];
      }
    })

    this._nodes.next([...updated]);
  }
}