---
name: general-create-condition
description: Create a new extension condition that controls when extensions are active. Use when you need to conditionally show or hide extensions based on application state (e.g., current section, user permissions, entity state, workspace context). Conditions are registered as extensions and referenced in manifest condition arrays.
allowed-tools: Read, Write, Edit, Grep, Glob
---

# Create Condition

Create a new extension condition that controls when extensions are active.

## What you need from the user

1. **What it checks** — What context or state determines the condition (e.g., current section, user role, entity state)
2. **Config properties** — What parameters consumers provide (e.g., `match`, `oneOf`)
3. **Context dependency** — Which context token provides the data to evaluate

## Files to create

Place in a `conditions/` subdirectory of the relevant feature:

```
my-feature/
└── conditions/
    └── my-condition/
        ├── constants.ts                     # Condition alias constant
        ├── types.ts                         # Config type + global declaration
        ├── my-condition.condition.ts         # Condition controller
        └── manifests.ts                     # Condition manifest
```

## Step 1: Define the alias constant

```typescript
// constants.ts
export const UMB_MY_CONDITION_ALIAS = 'Umb.Condition.MyCondition';
```

## Step 2: Define the config type

```typescript
// types.ts
import type { UMB_MY_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export interface MyConditionConfig extends UmbConditionConfigBase<typeof UMB_MY_CONDITION_ALIAS> {
	/**
	 * The value to match against.
	 * @example "Umb.Section.Content"
	 */
	match?: string;

	/**
	 * One or more values to match against (any match passes).
	 * @example ["Umb.Section.Content", "Umb.Section.Media"]
	 */
	oneOf?: Array<string>;
}

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbMyConditionConfig: MyConditionConfig;
	}
}
```

### Key rules

- Extend `UmbConditionConfigBase<typeof ALIAS_CONSTANT>` — this types the `alias` property
- The `declare global` block on `UmbExtensionConditionConfigMap` makes the config type-safe when used in extension manifests
- Use a unique key in the map
- Add JSDoc with `@example` — these feed into JSON schema generation

### Common config patterns

| Pattern | Use when |
|---------|----------|
| `match: string` | Single value match |
| `oneOf: Array<string>` | Any of multiple values |
| `allOf: Array<string>` | All values must match |
| `noneOf: Array<string>` | None of the values match |
| No config properties | Condition checks a boolean state (e.g., "is trashed", "is admin") |

## Step 3: Implement the condition controller

```typescript
// my-condition.condition.ts
import type { MyConditionConfig } from './types.js';
import { UMB_MY_CONTEXT } from '../../my-context.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbMyCondition
	extends UmbConditionBase<MyConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<MyConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_MY_CONTEXT, (context) => {
			this.observe(
				context.someObservable,
				(value) => {
					this.permitted = this.#check(value);
				},
				'_observeConditionValue',
			);
		});
	}

	#check(value: string | undefined): boolean {
		if (!value) return false;

		if (this.config.match) {
			return value === this.config.match;
		}

		if (this.config.oneOf) {
			return this.config.oneOf.includes(value);
		}

		return false;
	}
}

export { UmbMyCondition as api };
```

### Key rules

- Extend `UmbConditionBase<ConfigType>` — provides `permitted` property and `config` access
- Implement `UmbExtensionCondition`
- Constructor signature: `(host: UmbControllerHost, args: UmbConditionControllerArguments<ConfigType>)`
- Set `this.permitted = true/false` — the base class notifies the extension system on change
- Use `consumeContext()` + `observe()` for reactive conditions that update as state changes
- Export as `api` (named export) so the class can be imported by the manifest
- Use an explicit observer alias so re-evaluation replaces the previous subscription

### For simple boolean conditions (no config)

```typescript
export class UmbIsSomethingCondition
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_SOME_CONTEXT, (context) => {
			this.observe(context.isSomething, (value) => {
				this.permitted = value === true;
			});
		});
	}
}
```

## Step 4: Register the condition manifest

Conditions must load fast — always import the class directly (no lazy `() => import()`).

```typescript
// manifests.ts
import { UMB_MY_CONDITION_ALIAS } from './constants.js';
import { UmbMyCondition } from './my-condition.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'My Condition',
		alias: UMB_MY_CONDITION_ALIAS,
		api: UmbMyCondition,
	},
];
```

Add to the parent feature's `manifests.ts` so it bubbles up to the bundle.

## How consumers use the condition

In any extension manifest that supports conditions:

```typescript
{
	type: 'workspaceAction',
	alias: 'My.WorkspaceAction',
	name: 'My Action',
	conditions: [
		{
			alias: 'Umb.Condition.MyCondition',
			match: 'some-value',
		},
	],
}
```

All conditions in the array must be `permitted` for the extension to be active. They are AND-ed together.

## Checklist

- [ ] Alias constant exported from `constants.ts`
- [ ] Config type extends `UmbConditionConfigBase<typeof ALIAS>` with the alias constant
- [ ] Config type declared on global `UmbExtensionConditionConfigMap`
- [ ] Condition class extends `UmbConditionBase<ConfigType>` and implements `UmbExtensionCondition`
- [ ] Constructor calls `super(host, args)` and sets up context observation
- [ ] `this.permitted` is set reactively (via `observe()`) — not just once in the constructor
- [ ] Condition manifest registered with `type: 'condition'`
- [ ] Manifests bubbled up to the package-level `manifests.ts`
- [ ] Compiles: `npm run compile`
