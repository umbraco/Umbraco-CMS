import type { UmbAction } from '../action/index.js';
import { UmbActionBase } from '../action/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Interface for an entity action.
 * @export
 * @interface UmbEntityAction<RepositoryType>
 * @extends {UmbAction<RepositoryType>}
 * @template RepositoryType
 */
export interface UmbEntityAction<RepositoryType> extends UmbAction<RepositoryType> {
	/**
	 * The unique identifier of the entity.
	 * @type {string | null}
	 */
	unique: string | null;

	/**
	 * The href location, the action will act as a link.
	 * @returns {Promise<string | null | undefined>}
	 */
	getHref(): Promise<string | null | undefined>;

	/**
	 * The `execute` method, the action will act as a button.
	 * @returns {Promise<void>}
	 */
	execute(): Promise<void>;
}

/**
 * Base class for an entity action.
 * @export
 * @abstract
 * @class UmbEntityActionBase<RepositoryType>
 * @extends {UmbActionBase<RepositoryType>}
 * @implements {UmbEntityAction<RepositoryType>}
 * @template RepositoryType
 */
export abstract class UmbEntityActionBase<RepositoryType>
	extends UmbActionBase<RepositoryType>
	implements UmbEntityAction<RepositoryType>
{
	entityType: string;
	unique: string | null;
	repositoryAlias: string;

	constructor(host: UmbControllerHost, repositoryAlias: string, unique: string | null, entityType: string) {
		/**
		 * Creates an instance of UmbEntityActionBase<RepositoryType>.
		 * @param {UmbControllerHost} host
		 * @param {string} repositoryAlias
		 * @param {string} unique
		 * @param {string} entityType
		 * @memberof UmbEntityActionBase<RepositoryType>
		 */
		super(host, repositoryAlias);
		this.entityType = entityType;
		this.unique = unique;
		this.repositoryAlias = repositoryAlias;
	}

	/**
	 * By specifying the href, the action will act as a link.
	 * The `execute` method will not be called.
	 * @abstract
	 * @returns {string | null | undefined}
	 */
	public getHref(): Promise<string | null | undefined> {
		return Promise.resolve(undefined);
	}

	/**
	 * By specifying the `execute` method, the action will act as a button.
	 * @abstract
	 * @returns {Promise<void>}
	 */
	public execute(): Promise<void> {
		return Promise.resolve();
	}
}
