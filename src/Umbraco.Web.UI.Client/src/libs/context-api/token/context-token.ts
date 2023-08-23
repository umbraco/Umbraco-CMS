
export type UmbContextDiscriminator<BaseType, DiscriminatorResult extends BaseType> = (instance: BaseType) => instance is DiscriminatorResult;

export class UmbContextToken<BaseType = unknown, DiscriminatedType extends BaseType = never, ResultType extends BaseType = keyof DiscriminatedType extends object ? DiscriminatedType : BaseType> {

	#discriminator: UmbContextDiscriminator<BaseType, DiscriminatedType> | undefined;
	/**
	 * Get the type of the token
	 *
	 * @public
	 * @type      {T}
	 * @memberOf  UmbContextToken
	 * @example   `typeof MyToken.TYPE`
	 * @returns   undefined
	 */
	readonly TYPE: ResultType = undefined as never;

	/**
	 * @param alias   Unique identifier for the token
	 */
	constructor(protected alias: string, discriminator?: UmbContextDiscriminator<BaseType, DiscriminatedType>) {
		this.#discriminator = discriminator;
	}

	getDiscriminator(): UmbContextDiscriminator<BaseType, DiscriminatedType> | undefined {
		return this.#discriminator;
	}

	/**
	 * This method must always return the unique alias of the token since that
	 * will be used to look up the token in the injector.
	 *
	 * @returns the unique alias of the token
	 */
	toString(): string {
		return this.alias;
	}


}
