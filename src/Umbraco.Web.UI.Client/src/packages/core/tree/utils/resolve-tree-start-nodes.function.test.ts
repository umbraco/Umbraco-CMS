import { umbResolveTreeStartNodes } from './resolve-tree-start-nodes.function.js';
import { expect } from '@open-wc/testing';

const nodeA = { unique: 'a', entityType: 'test-entity-type' };
const nodeB = { unique: 'b', entityType: 'test-entity-type' };

describe('umbResolveTreeStartNodes', () => {
	it('resolves nothing when neither is given', () => {
		expect(umbResolveTreeStartNodes(undefined, undefined)).to.deep.equal({
			startNode: undefined,
			startNodes: undefined,
		});
	});

	it('resolves a single start node as the start node', () => {
		expect(umbResolveTreeStartNodes(nodeA, undefined)).to.deep.equal({ startNode: nodeA, startNodes: undefined });
	});

	it('resolves a single start node from the start nodes as the start node', () => {
		expect(umbResolveTreeStartNodes(undefined, [nodeA])).to.deep.equal({ startNode: nodeA, startNodes: undefined });
	});

	it('resolves multiple start nodes as the start nodes', () => {
		expect(umbResolveTreeStartNodes(undefined, [nodeA, nodeB])).to.deep.equal({
			startNode: undefined,
			startNodes: [nodeA, nodeB],
		});
	});

	it('lets the start nodes take precedence over the start node', () => {
		expect(umbResolveTreeStartNodes(nodeA, [nodeB])).to.deep.equal({ startNode: nodeB, startNodes: undefined });
	});

	it('falls back to the start node when the start nodes are empty', () => {
		expect(umbResolveTreeStartNodes(nodeA, [])).to.deep.equal({ startNode: nodeA, startNodes: undefined });
	});
});
