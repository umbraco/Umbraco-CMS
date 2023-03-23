import type { IRoute } from 'router-slot/model';
/*
type Optional<T> = { [P in keyof T]?: T[P] };

type IRouteWithComponentRoute = Exclude<IRoute, IComponentRoute>;
type IComponentRouteWithOptionalComponent = Omit<IComponentRoute, 'component'> &
	Optional<Pick<IComponentRoute, 'component'>>;

export type UmbRoute = IRouteWithComponentRoute | IComponentRouteWithOptionalComponent;
*/
export type UmbRoute = IRoute;
