﻿using System.Collections.Generic;
using Cowboy.Responses;
using Cowboy.Responses.Serialization;
using Cowboy.Routing;
using Cowboy.Routing.Constraints;
using Cowboy.Routing.Trie;

namespace Cowboy
{
    public class Bootstrapper
    {
        public Bootstrapper()
        {
            this.Modules = new List<Module>();
        }

        public List<Module> Modules { get; set; }

        public Engine Boot()
        {
            var moduleCatalog = new ModuleCatalog();
            foreach (var module in Modules)
            {
                moduleCatalog.RegisterModule(module);
            }

            var routeSegmentExtractor = new RouteSegmentExtractor();
            var routeDescriptionProvider = new RouteDescriptionProvider();
            var routeCache = new RouteCache(routeSegmentExtractor, routeDescriptionProvider);
            routeCache.BuildCache(moduleCatalog.GetAllModules());

            var routeConstraints = new List<IRouteSegmentConstraint>()
            {
                new AlphaRouteSegmentConstraint(),
                new BoolRouteSegmentConstraint(),
                new CustomDateTimeRouteSegmentConstraint(),
                new DateTimeRouteSegmentConstraint(),
                new DecimalRouteSegmentConstraint(),
                new GuidRouteSegmentConstraint(),
                new IntRouteSegmentConstraint(),
                new LengthRouteSegmentConstraint(),
                new LongRouteSegmentConstraint(),
                new MaxLengthRouteSegmentConstraint(),
                new MaxRouteSegmentConstraint(),
                new MinLengthRouteSegmentConstraint(),
                new MinRouteSegmentConstraint(),
                new RangeRouteSegmentConstraint(),
                new VersionRouteSegmentConstraint(),
            };
            var trieNodeFactory = new TrieNodeFactory(routeConstraints);
            var routeTrie = new RouteResolverTrie(trieNodeFactory);
            routeTrie.BuildTrie(routeCache);

            var serializers = new List<ISerializer>()
            {
                new JsonSerializer(),
                new XmlSerializer(),
            };
            var responseFormatterFactory = new ResponseFormatterFactory(serializers);
            var moduleBuilder = new ModuleBuilder(responseFormatterFactory);

            var routeResolver = new RouteResolver(moduleCatalog, moduleBuilder, routeTrie);

            var negotiator = new ResponseNegotiator();
            var routeInvoker = new RouteInvoker(negotiator);

            var contextFactory = new ContextFactory();
            var dispatcher = new RequestDispatcher(routeResolver, routeInvoker);
            var engine = new Engine(contextFactory, dispatcher);

            return engine;
        }
    }
}