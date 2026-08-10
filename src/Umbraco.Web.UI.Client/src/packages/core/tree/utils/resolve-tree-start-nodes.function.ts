import type { UmbTreeStartNode } from '../types.js';

/**
 * Resolves the start node configuration of a tree from a single start node and/or a set of start nodes.
 * A single resolved node is returned as `startNode`, so zero or one node always takes the single start node
 * path; only two or more nodes produce `startNodes`.
 * @param {UmbTreeStartNode | undefined} startNode - A single start node.
 * @param {Array<UmbTreeStartNode> | undefined} startNodes - A set of start nodes, taking precedence over `startNode`.
 * @returns {{ startNode?: UmbTreeStartNode; startNodes?: Array<UmbTreeStartNode> }} - The resolved configuration.
 */
export function umbResolveTreeStartNodes(
	startNode: UmbTreeStartNode | undefined,
	startNodes: Array<UmbTreeStartNode> | undefined,
): { startNode?: UmbTreeStartNode; startNodes?: Array<UmbTreeStartNode> } {
	const resolved = startNodes?.length ? startNodes : startNode ? [startNode] : [];

	if (resolved.length === 1) {
		return { startNode: resolved[0], startNodes: undefined };
	}

	if (resolved.length > 1) {
		return { startNode: undefined, startNodes: resolved };
	}

	return { startNode: undefined, startNodes: undefined };
}
