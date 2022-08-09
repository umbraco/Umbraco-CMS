import { BehaviorSubject, map, Observable } from 'rxjs';
import { NodeEntity, umbNodeData } from '../../mocks/data/node.data';

export class UmbNodeStore {
	private _nodes: BehaviorSubject<Array<NodeEntity>> = new BehaviorSubject(<Array<NodeEntity>>[]);
	public readonly nodes: Observable<Array<NodeEntity>> = this._nodes.asObservable();

	getById(id: number): Observable<NodeEntity | null> {
		// fetch from server and update store
		fetch(`/umbraco/backoffice/content/${id}`)
			.then((res) => res.json())
			.then((data) => {
				this._updateStore(data);
			});

		return this.nodes.pipe(map((nodes: Array<NodeEntity>) => nodes.find((node: NodeEntity) => node.id === id) || null));
	}

	// TODO: temp solution until we know where to get tree data from
	getAll(): Observable<Array<NodeEntity>> {
		const nodes = umbNodeData.getAll();
		this._nodes.next(nodes);
		return this.nodes;
	}

	// TODO: Use Node type, to not be specific about Document.
	// TODO: make sure UI somehow can follow the status of this action.
	save(data: NodeEntity[]): Promise<void> {
		// fetch from server and update store
		// TODO: use Fetcher API.
		let body: string;

		try {
			body = JSON.stringify(data);
		} catch (error) {
			console.error(error);
			return Promise.reject();
		}

		// TODO: Use node type to hit the right API, or have a general Node API?
		return fetch('/umbraco/backoffice/content/save', {
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
		const updated: NodeEntity[] = [...storedNodes];

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
