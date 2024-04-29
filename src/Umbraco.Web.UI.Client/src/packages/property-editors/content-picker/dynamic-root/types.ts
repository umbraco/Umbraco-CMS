export type UmbDynamicRoot<StepType extends UmbDynamicRootQueryStep = UmbDynamicRootQueryStep> = {
	originAlias: string;
	originKey?: string;
	querySteps?: Array<StepType>;
};

export type UmbDynamicRootQueryStep = {
	unique: string;
	alias: string;
};
